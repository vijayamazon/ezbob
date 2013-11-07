namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Repository;
	using Models;
	using Infrastructure.csrf;
	using Scorto.Web;
	using StructureMap;

    public class LoanOptionsController : Controller
	{
		private readonly ICustomerStatusHistoryRepository customerStatusHistoryRepository;
		private readonly CustomerStatusesRepository customerStatusesRepository;
        private readonly ILoanOptionsRepository _loanOptionsRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ICaisFlagRepository _caisFlagRepository;

		public LoanOptionsController(ILoanOptionsRepository loanOptionsRepository, ILoanRepository loanRepository, ICustomerStatusHistoryRepository customerStatusHistoryRepository, CustomerStatusesRepository customerStatusesRepository)
        {
            _loanOptionsRepository = loanOptionsRepository;
            _loanRepository = loanRepository;
            _caisFlagRepository = ObjectFactory.GetInstance<CaisFlagRepository>();
			this.customerStatusHistoryRepository = customerStatusHistoryRepository;
			this.customerStatusesRepository = customerStatusesRepository;
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonNetResult Index(int loanId)
        {
            var options = _loanOptionsRepository.GetByLoanId(loanId) ?? SetDefaultStatus(loanId);
            var loan = _loanRepository.Get(loanId);
            var flags = _caisFlagRepository.GetForStatusType();
            var model = new LoanOptionsViewModel(options, loan, flags);
            return this.JsonNet(model);
        }

        private LoanOptions SetDefaultStatus(int loanid)
        {
            var options = new LoanOptions
                              {
                                  AutoPayment = true,
                                  LatePaymentNotification = true,
                                  ReductionFee = true,
                                  StopSendingEmails = true,
                                  CaisAccountStatus = "Calculated value",
                                  LoanId = loanid
                              };
            return options;
        }

        [Transactional]
        [Ajax]
        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult Save(LoanOptions options)
         {
             if (options.ManulCaisFlag == "T")
                 options.ManulCaisFlag = "Calculated value";

             _loanOptionsRepository.SaveOrUpdate(options);

			if (options.CaisAccountStatus == "8")
			{
				Customer customer = _loanRepository.Get(options.LoanId).Customer;
				Loan triggeringLoan = null;
				
				// Update loan options
				foreach (Loan loan in customer.Loans)
				{
					if (loan.Id == options.LoanId)
					{
						triggeringLoan = loan;
						continue;
					}

					if (loan.Status == LoanStatus.PaidOff)
					{
						continue;
					}

					LoanOptions currentOptions = _loanOptionsRepository.GetByLoanId(loan.Id);
					if (currentOptions == null)
					{
						currentOptions = new LoanOptions
						{
							LoanId = loan.Id,
							AutoPayment = true,
							ReductionFee = true,
							LatePaymentNotification = true,
							StopSendingEmails = true,
							ManulCaisFlag = "Calculated value"
						};
					}

					currentOptions.CaisAccountStatus = "8";
					_loanOptionsRepository.SaveOrUpdate(currentOptions);
				}

				// Update customer status
				int prevStatus = customer.CollectionStatus.CurrentStatus.Id;
				customer.CollectionStatus.CurrentStatus = customerStatusesRepository.GetByName("Default");
				customer.CollectionStatus.CollectionDescription = string.Format("Triggered via loan options:{0}", triggeringLoan != null ? triggeringLoan.RefNumber : "unknown");

				// Update status history table
				var newEntry = new CustomerStatusHistory
					{
						Username = User.Identity.Name,
						Timestamp = DateTime.UtcNow,
						CustomerId = customer.Id,
						PreviousStatus = prevStatus,
						NewStatus = customer.CollectionStatus.CurrentStatus.Id
					};
				customerStatusHistoryRepository.SaveOrUpdate(newEntry);
			}

             return this.JsonNet(new { });
         }
    }
}
