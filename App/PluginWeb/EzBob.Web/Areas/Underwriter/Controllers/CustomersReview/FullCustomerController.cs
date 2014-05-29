namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using EZBob.DatabaseLib.Model.Database.Loans;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Models.Marketplaces;
	using Infrastructure;
	using Models;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;
	using Web.Models;
	using NHibernate;
	using System;

	public class FullCustomerController : Controller
	{
		private readonly ICustomerRepository _customers;
		private readonly ISession _session;
		private readonly ApplicationInfoModelBuilder _infoModelBuilder;
		private readonly WebMarketPlacesFacade _marketPlaces;
		private readonly CreditBureauModelBuilder _creditBureauModelBuilder;
		private readonly ProfileSummaryModelBuilder _summaryModelBuilder;
		private readonly FraudDetectionRepository _fraudDetectionLog;
		private readonly ApiCheckLogBuilder _apiCheckLogBuilder;
		private readonly MessagesModelBuilder _messagesModelBuilder;
		private readonly CustomerRelationsRepository _customerRelationsRepository;
		private readonly LoanRepository _loanRepository;
		private readonly CustomerAddressRepository customerAddressRepository;
		private readonly NHibernateRepositoryBase<MP_AlertDocument> _docRepo;
		private readonly IBugRepository _bugs;
		private readonly ServiceClient serviceClient;

		public FullCustomerController(
											ICustomerRepository customers,
											ISession session,
											ApplicationInfoModelBuilder infoModelBuilder,
											WebMarketPlacesFacade marketPlaces,
											CreditBureauModelBuilder creditBureauModelBuilder,
											ProfileSummaryModelBuilder summaryModelBuilder,
											FraudDetectionRepository fraudDetectionLog,
											ApiCheckLogBuilder apiCheckLogBuilder,
											MessagesModelBuilder messagesModelBuilder,
											CustomerRelationsRepository customerRelationsRepository,
											NHibernateRepositoryBase<MP_AlertDocument> docRepo,
											IBugRepository bugs, 
											LoanRepository loanRepository,
											CustomerAddressRepository customerAddressRepository)
		{
			_customers = customers;
			_session = session;
			_infoModelBuilder = infoModelBuilder;
			_marketPlaces = marketPlaces;
			_creditBureauModelBuilder = creditBureauModelBuilder;
			_summaryModelBuilder = summaryModelBuilder;
			_fraudDetectionLog = fraudDetectionLog;
			_apiCheckLogBuilder = apiCheckLogBuilder;
			_messagesModelBuilder = messagesModelBuilder;
			_customerRelationsRepository = customerRelationsRepository;
			_docRepo = docRepo;
			_bugs = bugs;
			_loanRepository = loanRepository;
			this.customerAddressRepository = customerAddressRepository;
			serviceClient = new ServiceClient();
		}

		//[Ajax]
		[HttpGet]
		public JsonResult Index(int id, string history = null)
		{
			var model = new FullCustomerModel();

			var customer = _customers.TryGet(id);

			if (customer == null)
			{
				model.State = "NotFound";
				return Json(model, JsonRequestBehavior.AllowGet);
			}

			var cr = customer.LastCashRequest;

			var pi = new PersonalInfoModel();
			pi.InitFromCustomer(customer, _session);
			model.PersonalInfoModel = pi;

			var m = new ApplicationInfoModel();
			_infoModelBuilder.InitApplicationInfo(m, customer, cr);
			model.ApplicationInfoModel = m;
			DateTime historyDate;
			model.Marketplaces = DateTime.TryParse(history, out historyDate)
									 ? _marketPlaces.GetMarketPlaceModels(customer, historyDate).ToList()
									 : _marketPlaces.GetMarketPlaceModels(customer, null).ToList();

			model.LoansAndOffers = new LoansAndOffers(customer);

			model.CreditBureauModel = _creditBureauModelBuilder.Create(customer, false, null);

			model.SummaryModel = _summaryModelBuilder.CreateProfile(customer);

			model.PaymentAccountModel = new PaymentsAccountModel(customer);

			model.MedalCalculations = new MedalCalculators(customer);
			var context = ObjectFactory.GetInstance<IWorkplaceContext>();
			PricingModelModelActionResult getPricingModelModelResponse = serviceClient.Instance.GetPricingModelModel(customer.Id, context.UserId);
			model.PricingModelCalculations = getPricingModelModelResponse.Value;

			int numberOfProperties = customer.PersonalInfo.ResidentialStatus == "Home owner" ? 1 : 0;
			int otherPropertiesCount = customerAddressRepository.GetAll().Count(a =>
										 a.Customer.Id == customer.Id &&
				                         (a.AddressType == CustomerAddressType.OtherPropertyAddress ||
				                         a.AddressType == CustomerAddressType.OtherPropertyAddressPrev));

			numberOfProperties += otherPropertiesCount;
			var currentAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault(x => x.AddressType == CustomerAddressType.PersonalAddress);
			Zoopla zoopla = null;
			if (currentAddress != null) zoopla = currentAddress.Zoopla.LastOrDefault();
			int zooplaValue = 0; 
			int experianMortgage = 0;
			int experianMortgageCount = 0;
			if (zoopla != null)
			{
				CrossCheckModel.GetZooplaAndMortgagesData(customer, zoopla.ZooplaEstimate, zoopla.AverageSoldPrice1Year, out zooplaValue, out experianMortgage, out experianMortgageCount);
			}

			model.Properties = new PropertiesModel(numberOfProperties, experianMortgageCount, zooplaValue, experianMortgage);

			DateTime? lastDateCheck;
			model.FraudDetectionLog = new FraudDetectionLogModel
				{
					FraudDetectionLogRows = _fraudDetectionLog.GetLastDetections(id, out lastDateCheck)
					                                          .Select(x => new FraudDetectionLogRowModel(x))
					                                          .OrderByDescending(x => x.Id)
					                                          .ToList(),
					LastCheckDate = lastDateCheck
				};

			model.ApiCheckLogs = _apiCheckLogBuilder.Create(customer);

			model.Messages = _messagesModelBuilder.Create(customer);

			var crm = new CustomerRelationsModelBuilder(_loanRepository, _customerRelationsRepository, _session);

			model.CustomerRelations = crm.Create(customer.Id);

			model.AlertDocs =
				(from d in _docRepo.GetAll() where d.Customer.Id == id select AlertDoc.FromDoc(d)).ToArray();

			model.Bugs = _bugs.GetAll()
				.Where(x => x.Customer.Id == customer.Id)
				.Select(x => BugModel.ToModel(x)).ToList();

			var builder = new CompanyScoreModelBuilder();
			model.CompanyScore = builder.Create(customer);

			model.ExperianDirectors = CrossCheckModel.GetExperianDirectors(customer);

			model.State = "Ok";

			model.MarketplacesHistory = _marketPlaces.GetMarketPlaceHistoryModel(customer).ToList();
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		private class FullCustomerModel
		{
			public PersonalInfoModel PersonalInfoModel { get; set; }
			public ApplicationInfoModel ApplicationInfoModel { get; set; }
			public List<MarketPlaceModel> Marketplaces { get; set; }
			public List<MarketPlaceHistoryModel> MarketplacesHistory { get; set; }
			public LoansAndOffers LoansAndOffers { get; set; }
			public CreditBureauModel CreditBureauModel { get; set; }
			public ProfileSummaryModel SummaryModel { get; set; }
			public PaymentsAccountModel PaymentAccountModel { get; set; }
			public MedalCalculators MedalCalculations { get; set; }
			public PricingModelModel PricingModelCalculations { get; set; }
			public PropertiesModel Properties { get; set; }
			public FraudDetectionLogModel FraudDetectionLog { get; set; }
			public List<ApiChecksLogModel> ApiCheckLogs { get; set; }
			public List<MessagesModel> Messages { get; set; }
			public CrmModel CustomerRelations { get; set; }
			public AlertDoc[] AlertDocs { get; set; }
			public List<BugModel> Bugs { get; set; }
			public string State { get; set; }
			public CompanyScoreModel CompanyScore { get; set; }
			public SortedSet<string> ExperianDirectors { get; set; }
		}
	}
}
