namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Infrastructure;
	using Infrastructure.Attributes;
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using PostcodeAnywhere;
	using System.Linq;
	using System.Web.Mvc;
	using Models;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Customer.Models;
	using ServiceClientProxy;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class PaymentAccountsController : Controller
	{
		private readonly CustomerRepository _customers;
		private readonly ICustomerMarketPlaceRepository _customerMarketplaces;
		private readonly ISortCodeChecker _sortCodeChecker;
		private readonly IPayPointFacade _payPointFacade;
		private readonly IWorkplaceContext _context;
		private readonly ServiceClient m_oServiceClient;

		public PaymentAccountsController(
			CustomerRepository customers,
			ICustomerMarketPlaceRepository customerMarketplaces,
			IPayPointFacade payPointFacade,
			IWorkplaceContext context
		)
		{
			_customers = customers;
			m_oServiceClient = new ServiceClient();
			_customerMarketplaces = customerMarketplaces;
			if (CurrentValues.Instance.PostcodeAnywhereEnabled)
			{
				_sortCodeChecker = new SortCodeChecker(CurrentValues.Instance.PostcodeAnywhereMaxBankAccountValidationAttempts);
			}
			else
			{
				_sortCodeChecker = new FakeSortCodeChecker();
			}
			_payPointFacade = payPointFacade;
			_context = context;
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id)
		{
			var customer = _customers.Get(id);
			var model = new PaymentsAccountModel(customer);
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[Transactional]
		public JsonResult SetDefaultCard(int customerId, int cardId)
		{
			var customer = _customers.Get(customerId);
			var card = customer.BankAccounts.SingleOrDefault(c => c.Id == cardId);
			customer.SetDefaultCard(card);
			return Json(new { }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[Transactional]
		public JsonResult PerformCheckBankAccount(int id, int cardid)
		{
			var customer = _customers.Get(id);
			var card = customer.BankAccounts.Single(b => b.Id == cardid);
			return CheckBankAccount(card);
		}

		[Ajax]
		[Transactional]
		public JsonResult CheckBankAccount(string bankAccount, string sortCode)
		{
			var card = new CardInfo(bankAccount, sortCode);
			return CheckBankAccount(card);
		}

		private JsonResult CheckBankAccount(CardInfo card)
		{
			string error = null;
			try
			{
				_sortCodeChecker.Check(card);
			}
			catch (UnknownSortCodeException)
			{
				error = "Sortcode was not found.";
			}
			catch (SortCodeNotFoundException)
			{
				error = "Sortcode was not found.";
			}
			catch (InvalidAccountNumberException)
			{
				error = "Invalid account number.";
			}

			if (!string.IsNullOrEmpty(error))
			{
				card.StatusInformation = error;
				return Json(new { error = error }, JsonRequestBehavior.AllowGet);
			}

			return Json(BankAccountModel.FromCard(card), JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[Transactional]
		public JsonResult TryAddBankAccount(int customerId, string bankAccount, string sortCode, BankAccountType accountType)
		{
			var customer = _customers.Get(customerId);

			if (customer.BankAccounts.Any(a => a.BankAccount == bankAccount && a.SortCode == sortCode))
			{
				return Json(new { error = "This bank account is already added." }, JsonRequestBehavior.AllowGet);
			}

			var card = new CardInfo() { BankAccount = bankAccount, SortCode = sortCode, Customer = customer, Type = accountType };
			try
			{
				_sortCodeChecker.Check(card);
			}
			catch (Exception)
			{
			}
			customer.BankAccounts.Add(card);
			customer.SetDefaultCard(card);

			return Json(new { r = card.Id }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult CheckForUpdatedStatus(int mpId)
		{
			return Json(new { status = _customerMarketplaces.Get(mpId).GetUpdatingStatus() }, JsonRequestBehavior.AllowGet);
		}

		public RedirectResult AddPayPoint(int id)
		{
			var oCustomer = _customers.Get(id);
			int payPointCardExpiryMonths = CurrentValues.Instance.PayPointCardExpiryMonths;
			DateTime cardMinExpiryDate = DateTime.UtcNow.AddMonths(payPointCardExpiryMonths);
			var callback = Url.Action("PayPointCallback", "PaymentAccounts", new { Area = "Underwriter", customerId = id, cardMinExpiryDate = FormattingUtils.FormatDateToString(cardMinExpiryDate), hideSteps = true }, "https");
			var url = _payPointFacade.GeneratePaymentUrl(oCustomer, 5m, callback);

			return Redirect(url);
		}

		[Transactional]
		[HttpGet]
		public ActionResult PayPointCallback(bool valid, string trans_id, string code, string auth_code, decimal? amount, string ip, string test_status, string hash, string message, string card_no, string customer, string expiry, int customerId)
		{
			if (test_status == "true")
			{
				// Use last 4 random digits as card number (to enable useful tests)
				string random4Digits = string.Format("{0}{1}", DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);
				if (random4Digits.Length > 4)
				{
					random4Digits = random4Digits.Substring(random4Digits.Length - 4);
				}
				card_no = random4Digits;
				expiry = string.Format("{0}{1}", "01", DateTime.Now.AddYears(2).Year.ToString().Substring(2, 2));
			}
			if (!valid || code != "A")
			{
				TempData["code"] = code;
				TempData["message"] = message;
				return View("Error");
			}

			if (!_payPointFacade.CheckHash(hash, Request.Url))
			{
				throw new Exception("check hash failed");
			}

			var cus = _customers.GetChecked(customerId);

			AddPayPointCardToCustomer(trans_id, card_no, cus, expiry);

			return View("PayPointAdded", amount ?? 0);
		}

		[Transactional]
		[HttpPost]
		public JsonResult AddPayPointCard(int customerId, string transactionid, string cardno, DateTime expiredate)
		{
			var customer = _customers.GetChecked(customerId);
			var expiry = expiredate.ToString("MMyy");

			AddPayPointCardToCustomer(transactionid, cardno, customer, expiry);

			return Json(new { });
		}

		private void AddPayPointCardToCustomer(string transactionid, string cardno, EZBob.DatabaseLib.Model.Database.Customer customer, string expiry)
		{
			customer.TryAddPayPointCard(transactionid, cardno, expiry, customer.PersonalInfo.Fullname);

			if (string.IsNullOrEmpty(customer.PayPointTransactionId))
			{
				SetPaypointDefaultCard(transactionid, customer.Id, cardno);
			}

			m_oServiceClient.Instance.PayPointAddedByUnderwriter(customer.Id, cardno, _context.User.FullName, _context.User.Id);
		}

		[Ajax]
		[Transactional]
		[HttpPost]
		public void SetPaypointDefaultCard(string transactionid, int customerId, string cardNo)
		{
			var customer = _customers.GetChecked(customerId);
			customer.PayPointTransactionId = transactionid;
			customer.CreditCardNo = cardNo;
		}

		[Ajax]
		[Transactional]
		[HttpPost]
		public void ChangeCustomerDefaultCardSelection(int customerId, bool state)
		{
			var customer = _customers.GetChecked(customerId);
			customer.DefaultCardSelectionAllowed = state;
		}
	}
}
