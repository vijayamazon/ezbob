namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System.Linq;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Models;
	using Infrastructure;
	using Infrastructure.Attributes;
	using PaymentServices.Calculators;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Models;

    public class CollectionStatusController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
	    private readonly CustomerStatusesRepository _customerStatusesRepository;
	    private readonly LoanOptionsRepository loanOptionsRepository;
	    private ConfigurationVariablesRepository configurationVariablesRepository;

		public CollectionStatusController(ICustomerRepository customerRepository, CustomerStatusesRepository customerStatusesRepository, LoanOptionsRepository loanOptionsRepository, ConfigurationVariablesRepository configurationVariablesRepository)
        {
            _customerRepository = customerRepository;
	        _customerStatusesRepository = customerStatusesRepository;
	        this.loanOptionsRepository = loanOptionsRepository;
			this.configurationVariablesRepository = configurationVariablesRepository;
        }

		[Ajax]
		public JsonResult GetStatuses()
		{
			return Json(_customerStatusesRepository.GetAll().ToList(), JsonRequestBehavior.AllowGet);
		}

        public JsonResult Index(int id, int currentStatus)
        {
            var customer = _customerRepository.Get(id);

            var collectionStatus = customer.CollectionStatus.CurrentStatus.Id == currentStatus
                ? customer.CollectionStatus : CreateDefaultCollectionStatusParameter(customer, currentStatus);

            var loans = customer.Loans.Select(l => LoanModel.FromLoan(l, new LoanRepaymentScheduleCalculator(l, null))).ToList();
            var loansNonClosed = loans.Where(l => l.DateClosed == null).ToList();

            var data = new CollectionStatusModel
                           {
                               CurrentStatus = collectionStatus.CurrentStatus.Id,
                               CollectionDescription = collectionStatus.CollectionDescription,
                               Items = loansNonClosed.Select(loan => new CollectionStatusItem
                               {
                                   LoanId = loan.Id,
                                   LoanRefNumber = loan.RefNumber
                               }).ToList()
                           };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private CollectionStatus CreateDefaultCollectionStatusParameter(Customer customer, int currentStatus)
        {
	        CustomerStatuses status = _customerStatusesRepository.Get(currentStatus);
	        return new CollectionStatus {CurrentStatus = status};
        }

        [HttpPost]
        [Transactional]
        [Ajax]
        [Permission(Name = "CustomerStatus")]
        public JsonResult Save(int customerId, int currentStatus, CollectionStatusModel collectionStatus)
        {
			int minDectForDefault = configurationVariablesRepository.GetByNameAsInt("MinDectForDefault");

            var customer = _customerRepository.Get(customerId);
			customer.CollectionStatus.CurrentStatus = _customerStatusesRepository.Get(currentStatus);
			if (customer.CollectionStatus.CurrentStatus.Name == "Default")
	        {
		        customer.CollectionStatus.CollectionDescription = collectionStatus.CollectionDescription;

				// Update loan options
				foreach (Loan loan in customer.Loans.Where(l => l.Status != LoanStatus.PaidOff && l.Balance >= minDectForDefault))
				{
					LoanOptions options = loanOptionsRepository.GetByLoanId(loan.Id);
					if (options == null)
					{
						options = new LoanOptions
							{
								LoanId = loan.Id,
								AutoPayment = true,
								ReductionFee = true,
								LatePaymentNotification = true,
								StopSendingEmails = true,
								ManulCaisFlag = "Calculated value"
							};
					}

					options.CaisAccountStatus = "8";
					loanOptionsRepository.SaveOrUpdate(options);
				}
	        }
	        return Json(new { });
        }
    }
}
