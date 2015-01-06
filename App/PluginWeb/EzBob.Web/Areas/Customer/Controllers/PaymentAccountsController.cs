namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Web.Mvc;
	using Code.Bank;
	using ConfigManager;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.AccountInfo;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure.Attributes;
	using PayPal;
	using PayPalDbLib.Models;
	using PayPalServiceLib;
	using Code.MpUniq;
	using Infrastructure;
	using Infrastructure.csrf;
	using ServiceClientProxy;
	using Web.Models.Strings;
	using PostcodeAnywhere;
	using StructureMap;
	using log4net;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class PaymentAccountsController : Controller
	{
		private readonly DatabaseDataHelper _helper;
		private readonly CustomerRepository _customers;
		private readonly IEzbobWorkplaceContext _context;
		private readonly ServiceClient m_oServiceClient;
		private readonly IMPUniqChecker _mpChecker;
		private readonly ISortCodeChecker _sortCodeChecker;
		private readonly IYodleeAccountChecker _yodleeAccountChecker;
		private static readonly ILog Log = LogManager.GetLogger(typeof(PaymentAccountsController));

		public PaymentAccountsController(
			DatabaseDataHelper helper,
			CustomerRepository customers,
			IEzbobWorkplaceContext context,
			IMPUniqChecker mpChecker,
			IYodleeAccountChecker yodleeAccountChecker
		) {
			_helper = helper;
			_customers = customers;
			_context = context;
			m_oServiceClient = new ServiceClient();
			_mpChecker = mpChecker;
			_yodleeAccountChecker = yodleeAccountChecker;
			if (CurrentValues.Instance.PostcodeAnywhereEnabled)
			{
				_sortCodeChecker = new SortCodeChecker(CurrentValues.Instance.PostcodeAnywhereMaxBankAccountValidationAttempts);
			}
			else
			{
				_sortCodeChecker = new FakeSortCodeChecker();
			}
		}

		[Transactional]
		public ViewResult Success(string request_token, string verification_code) {
			Log.InfoFormat("PayPal Add callback, token:{0} verification code:{1}", request_token, verification_code);

			if (string.IsNullOrEmpty(verification_code) && string.IsNullOrEmpty(request_token))
			{
				Log.InfoFormat("PayPal adding was canceled by customer");
				return View("PayPalCanceled");
			}

			var paypal = ObjectFactory.GetInstance<PayPalDatabaseMarketPlace>();

			var customer = _context.Customer;

			PayPalPermissionsGranted permissionsGranted;
			PayPalPersonalData personalData;
			try
			{
				permissionsGranted = PayPalServiceHelper.GetAccessToken(request_token, verification_code);
				personalData = PayPalServiceHelper.GetAccountInfo(permissionsGranted);
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

			int mpId = 0;

			Transactional.Execute(() => {
				mpId = SavePayPal(customer, permissionsGranted, personalData, paypal);
			});

			if (mpId > 0)
				m_oServiceClient.Instance.UpdateMarketplace(customer.Id, mpId, true, _context.UserId);

			return View(permissionsGranted);
		}

		private int SavePayPal(Customer customer, PayPalPermissionsGranted permissionsGranted, PayPalPersonalData personalData, PayPalDatabaseMarketPlace paypal)
		{
			var securityData = new PayPalSecurityData
			{
				PermissionsGranted = permissionsGranted,
				UserId = personalData.Email
			};

			var mp = _helper.SaveOrUpdateCustomerMarketplace(personalData.Email, paypal, securityData, customer);
			_helper.SaveOrUpdateAcctountInfo(mp, personalData);
			return mp.Id;
		}

		public JsonResult GetRequestPermissionsUrl()
		{
			try
			{
				var callback = Url.Action("Success", "PaymentAccounts", new { Area = "Customer" }, "https");
				var url = PayPalServiceHelper.GetRequestPermissionsUrl(callback);
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
				var response = PayPalServiceHelper.GetRequestPermissionsUrl(callback);

				return Redirect(response.Url);
			}
			catch (Exception e)
			{
				Log.Error("Adding paypal failed", e);
				return View("PPAttachError");
			}
		}

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult PayPalList()
		{
			var customer = _context.Customer;
			var paypal = new PayPalDatabaseMarketPlace();

			return Json(customer.CustomerMarketPlaces
				.Where(m => m.Marketplace.InternalId == paypal.InternalId)
				.Select(m => new { displayName = m.DisplayName }).ToArray(),
				JsonRequestBehavior.AllowGet
			);
		}

		[Transactional]
		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult BankAccountsListFormatted()
		{
			var customer = _context.Customer;

			if (!customer.HasBankAccount)
			{
				return Json(new[] { new { } }, JsonRequestBehavior.AllowGet);
			}

			return Json(new[] { new { displayName = "XXXX" + customer.BankAccount.AccountNumber.Substring(4) } }, JsonRequestBehavior.AllowGet);
		}

		[Transactional]
		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult AddBankAccount(string accountNumber, string sortCode, string BankAccountType)
		{
			try
			{
				var customer = _context.Customer;
				if (customer == null)
				{
					return Json(new { error = "Unknown customer" });
				}

				if (string.IsNullOrEmpty(accountNumber) || !Regex.IsMatch(accountNumber, @"^\d{8}$"))
				{
					return Json(new { error = "Invalid account number" });
				}

				if (string.IsNullOrEmpty(sortCode) || !Regex.IsMatch(sortCode, @"^\d{6}$"))
				{
					return Json(new { error = "Invalid sort code" });
				}

				var card = _sortCodeChecker.Check(customer, accountNumber, sortCode, BankAccountType);

				_yodleeAccountChecker.Check(customer, accountNumber, sortCode, BankAccountType);

				customer.BankAccount = new BankAccount
					{
						AccountNumber = accountNumber,
						SortCode = sortCode,
						Type = (BankAccountType)Enum.Parse(typeof(BankAccountType), BankAccountType)
					};

				customer.CurrentCard = card;
				_customers.Update(customer);

				return Json(new { msg = "Well done! You've added your bank account!" });
			}
			catch (SortCodeNotFoundException)
			{
				return Json(new { error = "Sort code was not found" });
			}
			catch (UnknownSortCodeException)
			{
				return Json(new { error = "Sort code was not found" });
			}
			catch (InvalidAccountNumberException)
			{
				return Json(new { error = "Account number is not valid" });
			}
			catch (NotValidSortCodeException)
			{
				return Json(new { error = "Sort code is not valid" });
			}
			catch (YodleeAccountNotFoundException)
			{
				return
					Json(new { error = "Account number doesn't match your linked bank account. Please use the same bank account number you linked before or go back and link a bank account that match the account number you have entered." });
			}
		}
	}
}
