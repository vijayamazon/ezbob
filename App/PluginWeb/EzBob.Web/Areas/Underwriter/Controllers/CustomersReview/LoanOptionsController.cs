namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System;
	using System.Web.Mvc;
	using ConfigManager;
	using DbConstants;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Repository;
	using Infrastructure.Attributes;
	using Models;
	using Infrastructure.csrf;
	using log4net;
	using StructureMap;

	public class LoanOptionsController : Controller {
		private readonly ICustomerStatusHistoryRepository customerStatusHistoryRepository;
		private readonly CustomerStatusesRepository customerStatusesRepository;
		private readonly ILoanOptionsRepository _loanOptionsRepository;
		private readonly ILoanRepository _loanRepository;
		private readonly ICaisFlagRepository _caisFlagRepository;
	    private readonly ILog Log = LogManager.GetLogger(typeof (LoanOptionsController));
		public LoanOptionsController(ILoanOptionsRepository loanOptionsRepository, ILoanRepository loanRepository, ICustomerStatusHistoryRepository customerStatusHistoryRepository, CustomerStatusesRepository customerStatusesRepository) {
			this._loanOptionsRepository = loanOptionsRepository;
			this._loanRepository = loanRepository;
			this._caisFlagRepository = ObjectFactory.GetInstance<CaisFlagRepository>();
			this.customerStatusHistoryRepository = customerStatusHistoryRepository;
			this.customerStatusesRepository = customerStatusesRepository;
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index(int loanId) {
			var options = this._loanOptionsRepository.GetByLoanId(loanId) ?? LoanOptions.GetDefault(loanId);
			var loan = this._loanRepository.Get(loanId);
			var flags = this._caisFlagRepository.GetForStatusType();
			var model = new LoanOptionsViewModel(options, loan, flags);
			return Json(model, JsonRequestBehavior.AllowGet);
		}


		[Transactional]
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Save(LoanOptions options) {
			if (options.ManualCaisFlag == "T")
				options.ManualCaisFlag = "Calculated value";

			this._loanOptionsRepository.SaveOrUpdate(options);

			if (options.CaisAccountStatus == "8") {
				int minDectForDefault = CurrentValues.Instance.MinDectForDefault;
				Customer customer = this._loanRepository.Get(options.LoanId).Customer;
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

					LoanOptions currentOptions = this._loanOptionsRepository.GetByLoanId(loan.Id) ?? new LoanOptions {
						LoanId = loan.Id,
						AutoPayment = true,
						ReductionFee = true,
						LatePaymentNotification = true,
						EmailSendingAllowed = false,
						MailSendingAllowed = false,
						SmsSendingAllowed = false,
						ManualCaisFlag = "Calculated value",
						AutoLateFees = true
					};

					currentOptions.CaisAccountStatus = "8";
					this._loanOptionsRepository.SaveOrUpdate(currentOptions);
				}

				// Update customer status
				CustomerStatuses prevStatus = customer.CollectionStatus.CurrentStatus;
				customer.CollectionStatus.CurrentStatus = this.customerStatusesRepository.Get((int)CollectionStatusNames.Default);
				customer.CollectionStatus.CollectionDescription = string.Format("Triggered via loan options:{0}", triggeringLoan != null ? triggeringLoan.RefNumber : "unknown");

				// Update status history table
				var newEntry = new CustomerStatusHistory {
					Username = User.Identity.Name,
					Timestamp = DateTime.UtcNow,
					CustomerId = customer.Id,
					PreviousStatus = prevStatus,
					NewStatus = customer.CollectionStatus.CurrentStatus,
				};
				this.customerStatusHistoryRepository.SaveOrUpdate(newEntry);
			}

            //TODO update new loan options table
			this.Log.DebugFormat("update loan options for loan {0}", options.LoanId);
            
			return Json(new { });
		}
	}
}
