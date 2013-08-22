namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model;
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
	using StructureMap;

    public class CollectionStatusController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly PayPointApi _payPointApi;
        private readonly ConfigurationVariablesRepository _configurationVariablesRepository;
        private readonly IAppCreator _appCreator;

        public CollectionStatusController(ICustomerRepository customerRepository, IAppCreator appCreator, PayPointApi paypoint)
        {
            _customerRepository = customerRepository;
            _appCreator = appCreator;
            _payPointApi = paypoint;
            _configurationVariablesRepository = ObjectFactory.GetInstance<ConfigurationVariablesRepository>();
        }

        public JsonNetResult Index(int id, CollectionStatusType currentStatus)
        {
            var customer = _customerRepository.Get(id);

            var collectionStatus = customer.CollectionStatus.CurrentStatus == currentStatus
                ? customer.CollectionStatus : CreateDefoultCollectionStatusParametr(customer, currentStatus);

            var loans = customer.Loans.Select(l => LoanModel.FromLoan(l, new LoanRepaymentScheduleCalculator(l, null))).ToList();
            var loansNonClosed = loans.Where(l => l.DateClosed == null).ToList();

            var data = new CollectionStatusModel
                           {
                               CurrentStatus = collectionStatus.CurrentStatus,
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

        private CollectionStatus CreateDefoultCollectionStatusParametr(EZBob.DatabaseLib.Model.Database.Customer customer, CollectionStatusType currentStatus)
        {
            var statParam = new CollectionStatus { CurrentStatus = currentStatus };
            statParam.CollectionDateOfDeclaration = DateTime.UtcNow;

            if (currentStatus == CollectionStatusType.Default || currentStatus == CollectionStatusType.Legal)
            {
                statParam.IsAddCollectionFee = true;
            }
            return statParam;
        }

        [HttpPost]
        [Transactional]
        [Ajax]
        [Permission(Name = "CustomerStatus")]
        public JsonNetResult Save(int customerId, CollectionStatusType currentStatus, CollectionStatusModel collectionStatus)
        {
            var customer = _customerRepository.Get(customerId);
            customer.CollectionStatus.CurrentStatus = currentStatus;
	        if (customer.CollectionStatus.CurrentStatus == CollectionStatusType.Default)
	        {
		        customer.CollectionStatus.CollectionDescription = collectionStatus.CollectionDescription;
	        }
	        return this.JsonNet(new { });
        }
    }
}
