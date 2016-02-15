namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using Code;
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
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using EzBob.Web.Code.MpUniq;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using ServiceClientProxy;
	using ActionResult = System.Web.Mvc.ActionResult;
	using PaymentServices.Calculators;

	public class PaymentAccountsController : Controller {
		private readonly CustomerRepository customersRepository;
		private readonly ICustomerMarketPlaceRepository customerMarketplacesReporsitory;
		private readonly ISortCodeChecker sortCodeChecker;
		private readonly IWorkplaceContext context;
		private readonly ServiceClient serviceClient;
		private readonly BankAccountUniqChecker bankAccountUniqChecker;

		public PaymentAccountsController(
			CustomerRepository customersRepository,
			ICustomerMarketPlaceRepository customerMarketplacesReporsitory,
			IWorkplaceContext context,
			ISortCodeChecker sortCodeChecker,
			BankAccountUniqChecker bankAccountUniqChecker) {
			this.customersRepository = customersRepository;
			this.serviceClient = new ServiceClient();
			this.customerMarketplacesReporsitory = customerMarketplacesReporsitory;

			this.context = context;
			this.sortCodeChecker = sortCodeChecker;
			this.bankAccountUniqChecker = bankAccountUniqChecker;
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int id) {
			var customer = this.customersRepository.Get(id);
			var model = new PaymentsAccountModel(customer);
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[Transactional]
		[Permission(Name = "DebitCardCustomerSelection")]
		public JsonResult SetDefaultCard(int customerId, int cardId) {
			var customer = this.customersRepository.Get(customerId);
			var card = customer.BankAccounts.SingleOrDefault(c => c.Id == cardId);
			customer.SetDefaultCard(card);
			return Json(new { }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[Transactional]
		[Permission(Name = "CheckBankAccount")]
		public JsonResult PerformCheckBankAccount(int id, int cardid) {
			var customer = this.customersRepository.Get(id);
			var card = customer.BankAccounts.Single(b => b.Id == cardid);
			return CheckBankAccount(card);
		}

		[Ajax]
		[Transactional]
		[Permission(Name = "CheckBankAccount")]
		public JsonResult CheckBankAccount(string bankAccount, string sortCode) {
			var card = new CardInfo(bankAccount, sortCode);
			return CheckBankAccount(card);
		}

		private JsonResult CheckBankAccount(CardInfo card) {
			string error = null;
			try {
				this.sortCodeChecker.Check(card);
			} catch (UnknownSortCodeException) {
				error = "Sortcode was not found.";
			} catch (SortCodeNotFoundException) {
				error = "Sortcode was not found.";
			} catch (InvalidAccountNumberException) {
				error = "Invalid account number.";
			}

			if (!string.IsNullOrEmpty(error)) {
				card.StatusInformation = error;
				return Json(new { error = error }, JsonRequestBehavior.AllowGet);
			}

			return Json(BankAccountModel.FromCard(card), JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[Transactional]
		[Permission(Name = "AddBankAccount")]
		public JsonResult TryAddBankAccount(int customerId, string bankAccount, string sortCode, BankAccountType accountType) {
			var customer = this.customersRepository.Get(customerId);

			int nCardID = customer.AddBankAccount(bankAccount, sortCode, accountType, this.bankAccountUniqChecker, this.sortCodeChecker);

			if (nCardID < 0) {
				switch (nCardID) {
				case -1:
					return Json(new { error = "Could not add bank account." }, JsonRequestBehavior.AllowGet);

				case -2:
					return Json(new { error = "This bank account is already added." }, JsonRequestBehavior.AllowGet);

				default:
					return Json(new { error = "Failed to add bank account." }, JsonRequestBehavior.AllowGet);
				} // switch
			} // if

			return Json(new { r = nCardID, blockBank = customer.BlockTakingLoan }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult CheckForUpdatedStatus(int mpId) {
			return Json(new { status = this.customerMarketplacesReporsitory.Get(mpId).GetUpdatingStatus() }, JsonRequestBehavior.AllowGet);
		}

		[Permission(Name="AddDebitCard")]
		public RedirectResult AddPayPoint(int id) {
			var oCustomer = this.customersRepository.Get(id);
			PayPointFacade payPointFacade = new PayPointFacade(oCustomer.MinOpenLoanDate(), oCustomer.CustomerOrigin.Name);
			int payPointCardExpiryMonths =payPointFacade.PayPointAccount.CardExpiryMonths;
			DateTime cardMinExpiryDate = DateTime.UtcNow.AddMonths(payPointCardExpiryMonths);
			var callback = Url.Action("PayPointCallback", "PaymentAccounts", new { Area = "Underwriter", customerId = id, cardMinExpiryDate = FormattingUtils.FormatDateToString(cardMinExpiryDate), hideSteps = true }, "https");

			var url = payPointFacade.GeneratePaymentUrl(oCustomer, 5.00m, callback);

			return Redirect(url);
		}

		[Transactional]
		[HttpGet]
		public ActionResult PayPointCallback(bool valid, string trans_id, string code, string auth_code, decimal? amount, string ip, string test_status, string hash, string message, string card_no, string customer, string expiry, int customerId) {
			var cus = this.customersRepository.GetChecked(customerId);
			if (test_status == "true") {
				// Use last 4 random digits as card number (to enable useful tests)
				string random4Digits = string.Format("{0}{1}", DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);
				if (random4Digits.Length > 4) {
					random4Digits = random4Digits.Substring(random4Digits.Length - 4);
				}
				card_no = random4Digits;
				expiry = string.Format("{0}{1}", "01", DateTime.Now.AddYears(2).Year.ToString().Substring(2, 2));
			}
			if (!valid || code != "A") {
				TempData["code"] = code;
				TempData["message"] = message;
				return View("Error");
			}

			PayPointFacade payPointFacade = new PayPointFacade(cus.MinOpenLoanDate(), cus.CustomerOrigin.Name);
			if (!payPointFacade.CheckHash(hash, Request.Url)) {
				throw new Exception("check hash failed");
			}

			bool paymentAdded = AddPayPointCardToCustomer(trans_id, card_no, cus, expiry, amount, payPointFacade.PayPointAccount);

			return View("PayPointAdded", new PaypointAddedModel { Amount = amount ?? 0, PaymentAdded = paymentAdded });
		}

		[Transactional]
		[HttpPost]
		[Permission(Name = "AddDebitCard")]
		public JsonResult AddPayPointCard(int customerId, string transactionid, string cardno, DateTime expiredate) {
			var customer = this.customersRepository.GetChecked(customerId);
			var expiry = expiredate.ToString("MMyy");
			PayPointFacade payPointFacade = new PayPointFacade(customer.MinOpenLoanDate(), customer.CustomerOrigin.Name);
			AddPayPointCardToCustomer(transactionid, cardno, customer, expiry, 0, payPointFacade.PayPointAccount);

			return Json(new { });
		}

		private bool AddPayPointCardToCustomer(string transactionid, string cardno, EZBob.DatabaseLib.Model.Database.Customer customer, string expiry, decimal? amount, PayPointAccount account) {
			bool paymentAdded = false;
			customer.TryAddPayPointCard(transactionid, cardno, expiry, customer.PersonalInfo.Fullname, account);

			bool hasOpenLoans = customer.Loans.Any(x => x.Status != LoanStatus.PaidOff);
			if (amount > 0 && hasOpenLoans) {

				Loan loan = customer.Loans.First(x => x.Status != LoanStatus.PaidOff);

				var nlPayment = new NL_Payments() {
					Amount = amount.Value,
					CreatedByUserID = this.context.UserId,
					//CreationTime = DateTime.UtcNow,
					//	LoanID = nlLoanId,
					//PaymentTime = DateTime.UtcNow,
					Notes = "Add paypoint card",
					//PaymentStatusID = (int)NLPaymentStatuses.Active,
					PaymentMethodID = (int)NLLoanTransactionMethods.SystemRepay,
					PaymentSystemType = NLPaymentSystemTypes.Paypoint
				};

				var f = new LoanPaymentFacade();
				f.PayLoan(loan, transactionid, amount.Value, Request.UserHostAddress, DateTime.UtcNow, "system-repay", false, null, nlPayment);

				paymentAdded = true;
			}

			if (amount > 0 && !hasOpenLoans) {
				this.serviceClient.Instance.PayPointAddedWithoutOpenLoan(customer.Id, this.context.UserId, amount.Value, transactionid);
			}

			this.serviceClient.Instance.PayPointAddedByUnderwriter(customer.Id, cardno, this.context.User.FullName, this.context.User.Id);

			return paymentAdded;
		}

		[Ajax]
		[Transactional]
		[HttpPost]
		[Permission(Name = "DebitCardCustomerSelection")]
		public void SetPaypointDefaultCard(string transactionid, int customerId, string cardNo) {
			var customer = this.customersRepository.GetChecked(customerId);
			var defaultCard = customer.PayPointCards.FirstOrDefault(x => x.TransactionId == transactionid);
			if (defaultCard == null) {
				throw new Exception("Paypoint card not found");
			}

			foreach (var card in customer.PayPointCards) {
				card.IsDefaultCard = false;
			}
			defaultCard.IsDefaultCard = true;
		}

		[Ajax]
		[Transactional]
		[HttpPost]
		[Permission(Name = "DebitCardCustomerSelection")]
		public void ChangeCustomerDefaultCardSelection(int customerId, bool state) {
			var customer = this.customersRepository.GetChecked(customerId);
			customer.DefaultCardSelectionAllowed = state;
		}
	}
}
