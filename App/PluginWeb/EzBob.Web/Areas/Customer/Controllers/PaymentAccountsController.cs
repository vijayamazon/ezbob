namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web.Mvc;
	using Code.ApplicationCreator;
	using Code.Bank;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.AccountInfo;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using CommonLib;
	using PayPal;
	using PayPalDbLib.Models;
	using PayPalServiceLib;
	using Code.MpUniq;
	using Infrastructure;
	using Infrastructure.csrf;
	using Web.Models.Strings;
	using NHibernate;
	using PostcodeAnywhere;
	using Scorto.Web;
	using StructureMap;
	using log4net;

	public class PaymentAccountsController : Controller
	{
		private readonly DatabaseDataHelper _helper;
		private readonly CustomerRepository _customers;
		private readonly IEzbobWorkplaceContext _context;
		private readonly IAppCreator _creator;
		private readonly ISession _session;
		private readonly IMPUniqChecker _mpChecker;
		private readonly ISortCodeChecker _sortCodeChecker;
		private readonly IYodleeAccountChecker _yodleeAccountChecker;
		private static readonly ILog Log = LogManager.GetLogger(typeof(PaymentAccountsController));
		private readonly IPayPalConfig _payPalConfig;

		public PaymentAccountsController(
			DatabaseDataHelper helper,
			CustomerRepository customers,
			IEzbobWorkplaceContext context,
			IAppCreator creator,
			ISession session,
			IMPUniqChecker mpChecker,
			ISortCodeChecker sortCodeChecker,
			IYodleeAccountChecker yodleeAccountChecker)
		{
			_helper = helper;
			_customers = customers;
			_context = context;
			_creator = creator;
			_session = session;
			_mpChecker = mpChecker;
			_sortCodeChecker = sortCodeChecker;
			_yodleeAccountChecker = yodleeAccountChecker;
			_payPalConfig = ObjectFactory.GetInstance<IPayPalConfig>();
		}

		public ActionResult Index()
		{
			return View();
		}

		[Transactional]
		public ViewResult Success(string request_token, string verification_code)
		{
			if (string.IsNullOrEmpty(verification_code) && string.IsNullOrEmpty(request_token))
			{
				Log.InfoFormat("PayPal adding was canceled by customer");
				return View("PayPalCanceled");
			}

			var paypal = ObjectFactory.GetInstance<PayPalDatabaseMarketPlace>();

			var customer = _context.Customer;

			PayPalPermissionsGranted permissionsGranted = PayPalServiceHelper.GetAccessToken(_payPalConfig, request_token, verification_code);
			PayPalPersonalData personalData;
			try
			{
				personalData = PayPalServiceHelper.GetAccountInfo(_payPalConfig, permissionsGranted);
				_mpChecker.Check(paypal.InternalId, customer, personalData.Email);
			}
			catch (PayPalException e)
			{
				Log.Warn(e);
				return View("Error", (object)string.Join("<br />", e.ErrorDetails.Select(x => x.message).ToList()));
			}
			catch (MarketPlaceAddedByThisCustomerException)
			{
				return View("Error", (object)DbStrings.PayPalAddedByYou);
			}
			catch (MarketPlaceIsAlreadyAddedException)
			{
				return View("Error", (object)DbStrings.AccountAlreadyExixtsInDb);
			}

			var securityData = new PayPalSecurityData
				{
					PermissionsGranted = permissionsGranted,
					UserId = personalData.Email
				};

			var mp = _helper.SaveOrUpdateCustomerMarketplace(personalData.Email, paypal, securityData, customer);
			_helper.SaveOrUpdateAcctountInfo(mp, personalData);
			_session.Flush();
			_creator.CustomerMarketPlaceAdded(_context.Customer, mp.Id);

			return View(permissionsGranted);
		}

		[Transactional]
		public JsonResult BasicPersonal()
		{
			var customer = _context.Customer;
			var paypal = new PayPalDatabaseMarketPlace();

			var data = customer.CustomerMarketPlaces.First(m => m.Marketplace.InternalId == paypal.InternalId).SecurityData;
			var securityData = SerializeDataHelper.DeserializeType<PayPalSecurityData>(data);
			var perm = securityData.PermissionsGranted;
			throw new NotImplementedException();
			//var response = _paypalFacade.GetBasicPersonal(perm);

			//return Json(response, JsonRequestBehavior.AllowGet);
		}

		public JsonResult GetRequestPermissionsUrl()
		{
			try
			{
				var callback = Url.Action("Success", "PaymentAccounts", new { Area = "Customer" }, "https");
				var url = PayPalServiceHelper.GetRequestPermissionsUrl(_payPalConfig, callback);
				return Json(new { url = url }, JsonRequestBehavior.AllowGet);
			}
			catch (PayPalException ex)
			{
				return Json(new { error = ex.ErrorDetails }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		public ActionResult AttachPayPal()
		{
			try
			{
				var callback = Url.Action("Success", "PaymentAccounts", new { Area = "Customer" }, "https");
				var response = PayPalServiceHelper.GetRequestPermissionsUrl(_payPalConfig, callback);

				return Redirect(response.Url);
			}
			catch (Exception e)
			{
				Log.Error("Adding paypal failed", e);
				return View("PPAttachError");
			}
		}

		[Transactional]
		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult PayPalList()
		{
			var customer = _context.Customer;
			var paypal = new PayPalDatabaseMarketPlace();
			return this.JsonNet(customer.CustomerMarketPlaces
										.Where(m => m.Marketplace.InternalId == paypal.InternalId)
										.Select(m => new { displayName = m.DisplayName }).ToArray()
										);
		}

		[Transactional]
		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult BankAccountsList()
		{
			var customer = _context.Customer;

			if (!customer.HasBankAccount)
			{
				return this.JsonNet(new[] { new { } });
			}

			return this.JsonNet(new[] { new { displayName = customer.BankAccount.AccountNumber } });
		}

		[Transactional]
		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult BankAccountsListFormatted()
		{
			var customer = _context.Customer;

			if (!customer.HasBankAccount)
			{
				return this.JsonNet(new[] { new { } });
			}

			return this.JsonNet(new[] { new { displayName = "XXXX" + customer.BankAccount.AccountNumber.Substring(4) } });
		}

		[Transactional]
		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult AddBankAccount(string accountNumber, string sortCode, string BankAccountType)
		{
			try
			{
				int customerId = _context.User.Id;
				var customer = _customers.Get(customerId);
				if (customer == null)
				{
					return this.JsonNet(new { error = "Unknown customer" });
				}

				if (string.IsNullOrEmpty(accountNumber) || !Regex.IsMatch(accountNumber, @"^\d{8}$"))
				{
					return this.JsonNet(new { error = "Invalid account number" });
				}

				if (string.IsNullOrEmpty(sortCode) || !Regex.IsMatch(sortCode, @"^\d{6}$"))
				{
					return this.JsonNet(new { error = "Invalid sort code" });
				}

				var card = _sortCodeChecker.Check(customer, accountNumber, sortCode, BankAccountType);

				_yodleeAccountChecker.Check(customer, accountNumber, sortCode, BankAccountType);

				customer.BankAccount = new BankAccount()
					{
						AccountNumber = accountNumber,
						SortCode = sortCode,
						Type = (BankAccountType)Enum.Parse(typeof(BankAccountType), BankAccountType)
					};

				customer.CurrentCard = card;
				_customers.Update(customer);

				return this.JsonNet(new { msg = "Well done! You've added your bank account!" });
			}
			catch (SortCodeNotFoundException)
			{
				return this.JsonNet(new { error = "Sort code was not found" });
			}
			catch (UnknownSortCodeException)
			{
				return this.JsonNet(new { error = "Sort code was not found" });
			}
			catch (InvalidAccountNumberException)
			{
				return this.JsonNet(new { error = "Account number is not valid" });
			}
			catch (NotValidSortCodeException)
			{
				return this.JsonNet(new { error = "Sort code is not valid" });
			}
			catch (YodleeAccountNotFoundException)
			{
				return
					this.JsonNet(new { error = "Account number doesn't match your linked bank account. Please use the same bank account number you linked before or go back and link a bank account that match the account number you have entered." });
			}
		}
	}
}
