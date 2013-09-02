namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EzBob.Models;
	using ApplicationCreator;
	using Infrastructure;
	using PaymentServices.Calculators;
	using PaymentServices.PayPoint;
	using Scorto.Web;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Models;

    public class CollectionStatusController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
	    private readonly CustomerStatusesRepository _customerStatusesRepository;

        public CollectionStatusController(ICustomerRepository customerRepository, CustomerStatusesRepository customerStatusesRepository, IAppCreator appCreator, PayPointApi paypoint)
        {
            _customerRepository = customerRepository;
	        _customerStatusesRepository = customerStatusesRepository;
        }

		[Ajax]
		public JsonNetResult GetStatuses()
		{
			return this.JsonNet(_customerStatusesRepository.GetAll().ToList());
		}

        public JsonNetResult Index(int id, int currentStatus)
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
                               CollectionDateOfDeclaration = collectionStatus.CollectionDateOfDeclaration !=null ? collectionStatus.CollectionDateOfDeclaration.ToString() : DateTime.UtcNow.ToString(),
                               Items = loansNonClosed.Select(loan => new CollectionStatusItem()
                               {
                                   IsAddCollectionFee = loan.Charge != null || loan.Status == LoanStatus.Late.ToString(),
                                   CollectionFee = loan.Charge!=null ?  loan.Charge.Amount : (loan.Balance * 10) / 100,
                                   LoanId = loan.Id,
                                   LoanRefNumber = loan.RefNumber
                               }).ToList()
                           };

            return this.JsonNet(data);
        }

        private CollectionStatus CreateDefaultCollectionStatusParameter(Customer customer, int currentStatus)
        {
	        CustomerStatuses status = _customerStatusesRepository.Get(currentStatus);
            var statParam = new CollectionStatus
	            {
		            CurrentStatus = status,
		            CollectionDateOfDeclaration = DateTime.UtcNow
	            };

			if (status!= null && (status.Name == "Default" || status.Name == "Legal"))
            {
                statParam.IsAddCollectionFee = true;
            }
            return statParam;
        }

        [HttpPost]
        [Transactional]
        [Ajax]
        [Permission(Name = "CustomerStatus")]
        public JsonNetResult Save(int customerId, int currentStatus, CollectionStatusModel collectionStatus)
        {
            var customer = _customerRepository.Get(customerId);
			customer.CollectionStatus.CurrentStatus = _customerStatusesRepository.Get(currentStatus);
			if (customer.CollectionStatus.CurrentStatus.Name == "Default")
	        {
		        customer.CollectionStatus.CollectionDescription = collectionStatus.CollectionDescription;
	        }
	        return this.JsonNet(new { });
        }
    }
}
