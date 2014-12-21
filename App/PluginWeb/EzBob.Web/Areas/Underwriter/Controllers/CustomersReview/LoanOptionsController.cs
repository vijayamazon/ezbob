namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System;
	using System.Web.Mvc;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Repository;
	using Infrastructure.Attributes;
	using Models;
	using Infrastructure.csrf;
	using StructureMap;

	public class LoanOptionsController : Controller {
		private readonly ICustomerStatusHistoryRepository customerStatusHistoryRepository;
		private readonly CustomerStatusesRepository customerStatusesRepository;
		private readonly ILoanOptionsRepository _loanOptionsRepository;
		private readonly ILoanRepository _loanRepository;
		private readonly ICaisFlagRepository _caisFlagRepository;

		public LoanOptionsController(ILoanOptionsRepository loanOptionsRepository, ILoanRepository loanRepository, ICustomerStatusHistoryRepository customerStatusHistoryRepository, CustomerStatusesRepository customerStatusesRepository) {
			_loanOptionsRepository = loanOptionsRepository;
			_loanRepository = loanRepository;
			_caisFlagRepository = ObjectFactory.GetInstance<CaisFlagRepository>();
			this.customerStatusHistoryRepository = customerStatusHistoryRepository;
			this.customerStatusesRepository = customerStatusesRepository;
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int loanId) {
			var options = _loanOptionsRepository.GetByLoanId(loanId) ?? SetDefaultStatus(loanId);
			var loan = _loanRepository.Get(loanId);
			var flags = _caisFlagRepository.GetForStatusType();
			var model = new LoanOptionsViewModel(options, loan, flags);
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[NonAction]
		private LoanOptions SetDefaultStatus(int loanid) {
			var options = new LoanOptions {
				AutoPayment = true,
				LatePaymentNotification = true,
				ReductionFee = true,
				EmailSendingAllowed = false,
				MailSendingAllowed = false,
				SmsSendingAllowed = false,
				CaisAccountStatus = "Calculated value",
				LoanId = loanid
			};
			return options;
		}

		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Save(LoanOptions options) {
			if (options.ManualCaisFlag == "T")
				options.ManualCaisFlag = "Calculated value";

			_loanOptionsRepository.SaveOrUpdate(options);

			if (options.CaisAccountStatus == "8") {
				int minDectForDefault = CurrentValues.Instance.MinDectForDefault;
				Customer customer = _loanRepository.Get(options.LoanId).Customer;
				Loan triggeringLoan = null;

				// Update loan options
				foreach (Loan loan in customer.Loans) {
					if (loan.Id == options.LoanId) {
						triggeringLoan = loan;
						continue;
					}

					if (loan.Status == LoanStatus.PaidOff || loan.Balance < minDectForDefault) {
						continue;
					}

					LoanOptions currentOptions = _loanOptionsRepository.GetByLoanId(loan.Id);
					if (currentOptions == null) {
						currentOptions = new LoanOptions {
							LoanId = loan.Id,
							AutoPayment = true,
							ReductionFee = true,
							LatePaymentNotification = true,
							EmailSendingAllowed = false,
							MailSendingAllowed = false,
							SmsSendingAllowed = false,
							ManualCaisFlag = "Calculated value"
						};
					}

					currentOptions.CaisAccountStatus = "8";
					_loanOptionsRepository.SaveOrUpdate(currentOptions);
				}

				// Update customer status
				CustomerStatuses prevStatus = customer.CollectionStatus.CurrentStatus;
				customer.CollectionStatus.CurrentStatus = customerStatusesRepository.Get((int)CollectionStatusNames.Default);
				customer.CollectionStatus.CollectionDescription = string.Format("Triggered via loan options:{0}", triggeringLoan != null ? triggeringLoan.RefNumber : "unknown");

				// Update status history table
				var newEntry = new CustomerStatusHistory {
					Username = User.Identity.Name,
					Timestamp = DateTime.UtcNow,
					CustomerId = customer.Id,
					PreviousStatus = prevStatus,
					NewStatus = customer.CollectionStatus.CurrentStatus,
				};
				customerStatusHistoryRepository.SaveOrUpdate(newEntry);
			}

			return Json(new { });
		}
	}
}
