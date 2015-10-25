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
	using Ezbob.Backend.Models;
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
		private readonly DatabaseDataHelper dbHelper;
		private readonly CustomerRepository customersRepository;
		private readonly IEzbobWorkplaceContext context;
		private readonly ServiceClient serviceClient;
		private readonly IMPUniqChecker mpChecker;
		private readonly ISortCodeChecker sortCodeChecker;
		private readonly IYodleeAccountChecker yodleeAccountChecker;
		private readonly BankAccountUniqChecker bankAccountUniqChecker;
		private static readonly ILog Log = LogManager.GetLogger(typeof(PaymentAccountsController));

		public PaymentAccountsController(
			DatabaseDataHelper dbHelper,
			CustomerRepository customersRepository,
			IEzbobWorkplaceContext context,
			IMPUniqChecker mpChecker,
			IYodleeAccountChecker yodleeAccountChecker, 
			BankAccountUniqChecker bankAccountUniqChecker,
			ISortCodeChecker sortCodeChecker) {
			this.dbHelper = dbHelper;
			this.customersRepository = customersRepository;
			this.context = context;
			this.serviceClient = new ServiceClient();
			this.mpChecker = mpChecker;
			this.yodleeAccountChecker = yodleeAccountChecker;
			this.bankAccountUniqChecker = bankAccountUniqChecker;
			this.sortCodeChecker = sortCodeChecker;
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

			var customer = this.context.Customer;

			PayPalPermissionsGranted permissionsGranted;
			PayPalPersonalData personalData;
			try
			{
				permissionsGranted = PayPalServiceHelper.GetAccessToken(request_token, verification_code);
				personalData = PayPalServiceHelper.GetAccountInfo(permissionsGranted);
				this.mpChecker.Check(paypal.InternalId, customer, personalData.Email);
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
				this.serviceClient.Instance.UpdateMarketplace(customer.Id, mpId, true, this.context.UserId);

			return View(permissionsGranted);
		}

		private int SavePayPal(Customer customer, PayPalPermissionsGranted permissionsGranted, PayPalPersonalData personalData, PayPalDatabaseMarketPlace paypal)
		{
			var securityData = new PayPalSecurityData
			{
				PermissionsGranted = permissionsGranted,
				UserId = personalData.Email
			};

			var mp = this.dbHelper.SaveOrUpdateCustomerMarketplace(personalData.Email, paypal, securityData, customer);
			this.dbHelper.SaveOrUpdateAcctountInfo(mp, personalData);
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
				Log.Warn("Adding paypal failed", e);
				return View("PPAttachError");
			}
		}

		[HttpGet]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult PayPalList()
		{
			var customer = this.context.Customer;
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
			var customer = this.context.Customer;

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
			var customer = this.context.Customer;
			if (customer == null) {
				return Json(new { error = "Unknown customer" });
			}

			try
			{
				if (string.IsNullOrEmpty(accountNumber) || !Regex.IsMatch(accountNumber, @"^\d{8}$"))
				{
					return Json(new { error = "Invalid account number" });
				}

				if (string.IsNullOrEmpty(sortCode) || !Regex.IsMatch(sortCode, @"^\d{6}$"))
				{
					return Json(new { error = "Invalid sort code" });
				}

				var card = this.sortCodeChecker.Check(customer, accountNumber, sortCode, BankAccountType);

				this.yodleeAccountChecker.Check(customer, accountNumber, sortCode, BankAccountType);

				customer.BankAccount = new BankAccount
					{
						AccountNumber = accountNumber,
						SortCode = sortCode,
						Type = (BankAccountType)Enum.Parse(typeof(BankAccountType), BankAccountType)
					};

				customer.CurrentCard = card;
				this.customersRepository.Update(customer);

				if (card != null) {
					this.bankAccountUniqChecker.Check(customer.Id, card);
				}

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
			} catch (BankAccountIsAlreadyAddedException) {
				customer.BlockTakingLoan = true;
				this.serviceClient.Instance.FraudChecker(customer.Id, FraudMode.FullCheck);
				this.serviceClient.Instance.CustomerBankAccountIsAlreadyAddedEmail(customer.Id);
				return Json(new { blockBank = true, msg = "Funds transfer is in process." }, JsonRequestBehavior.AllowGet);
			}
		}
	}
}
