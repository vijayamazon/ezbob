namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Diagnostics;
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
	using Ezbob.Backend.Models;
	using Infrastructure;
	using Models;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;
	using Web.Models;
	using NHibernate;
	using System;
	using log4net;

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
		private readonly LandRegistryRepository landRegistryRepository;

		private readonly NHibernateRepositoryBase<MP_AlertDocument> _docRepo;
		private readonly IBugRepository _bugs;
		private readonly ServiceClient serviceClient;
		private static readonly ILog Log = LogManager.GetLogger(typeof(FullCustomerController));
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
											CustomerAddressRepository customerAddressRepository,
											LandRegistryRepository landRegistryRepository)
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
			this.landRegistryRepository = landRegistryRepository;
			serviceClient = new ServiceClient();
		}

		//[Ajax]
		[HttpGet]
		public JsonResult Index(int id, string history = null)
		{
			Log.DebugFormat("Build full customer model begin for customer {0} and history = {1}", id, history);
			var model = new FullCustomerModel();

			var customer = _customers.TryGet(id);

			if (customer == null)
			{
				model.State = "NotFound";
				return Json(model, JsonRequestBehavior.AllowGet);
			}

			var cr = customer.LastCashRequest;
			Stopwatch sw = Stopwatch.StartNew();
			Stopwatch totalSw = Stopwatch.StartNew();
			var pi = new PersonalInfoModel();
			pi.InitFromCustomer(customer, _session);
			model.PersonalInfoModel = pi;
			sw.Stop();
			var personalTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			var m = new ApplicationInfoModel();
			_infoModelBuilder.InitApplicationInfo(m, customer, cr);
			model.ApplicationInfoModel = m;
			sw.Stop();
			var appTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			DateTime historyDate;
			bool bHasHistoryDate = DateTime.TryParse(history, out historyDate);

			var ar = serviceClient.Instance.CalculateModelsAndAffordability(id, bHasHistoryDate ? historyDate : (DateTime?)null);
			model.MarketPlaces = JsonConvert.DeserializeObject<MarketPlaceModel[]>(ar.Models).ToList();
			model.Affordability = ar.Affordability.ToList();
			sw.Stop();
			var mpTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			model.LoansAndOffers = new LoansAndOffers(customer);
			sw.Stop();
			var loanTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			model.CreditBureauModel = _creditBureauModelBuilder.Create(customer, false, null);
			sw.Stop();
			var expTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();

			model.SummaryModel = _summaryModelBuilder.CreateProfile(customer);
			sw.Stop();
			var summaryTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			model.PaymentAccountModel = new PaymentsAccountModel(customer);
			sw.Stop();
			var paymentTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			model.MedalCalculations = new MedalCalculators(customer);
			sw.Stop();
			var medalTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			var context = ObjectFactory.GetInstance<IWorkplaceContext>();
			try
			{
				PricingModelModelActionResult getPricingModelModelResponse = serviceClient.Instance.GetPricingModelModel(
					customer.Id, context.UserId, "Basic");
				model.PricingModelCalculations = getPricingModelModelResponse.Value;
			}
			catch (Exception ex)
			{
				Log.ErrorFormat("Failed to load pricing model \n{0}", ex);
			}
			sw.Stop();
			var pricingTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();

			model.Properties = PropertiesController.GetPropertiesModelData(customer, customerAddressRepository, landRegistryRepository);
			
			sw.Stop();
			var propTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			DateTime? lastDateCheck;
			model.FraudDetectionLog = new FraudDetectionLogModel
				{
					FraudDetectionLogRows = _fraudDetectionLog.GetLastDetections(id, out lastDateCheck)
					                                          .Select(x => new FraudDetectionLogRowModel(x))
					                                          .OrderByDescending(x => x.Id)
					                                          .ToList(),
					LastCheckDate = lastDateCheck
				};
			sw.Stop();
			var fraudTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			model.ApiCheckLogs = _apiCheckLogBuilder.Create(customer);
			sw.Stop();
			var apiTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			model.Messages = _messagesModelBuilder.Create(customer);
			sw.Stop();
			var messagesTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			var crm = new CustomerRelationsModelBuilder(_loanRepository, _customerRelationsRepository, _session);

			model.CustomerRelations = crm.Create(customer.Id);
			sw.Stop();
			var crmTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			model.AlertDocs =
				(from d in _docRepo.GetAll() where d.Customer.Id == id select AlertDoc.FromDoc(d)).ToArray();
			sw.Stop();
			var docsTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			model.Bugs = _bugs.GetAll()
				.Where(x => x.Customer.Id == customer.Id)
				.Select(x => BugModel.ToModel(x)).ToList();
			sw.Stop();
			var bugsTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			var builder = new CompanyScoreModelBuilder();
			model.CompanyScore = builder.Create(customer);
			sw.Stop();
			var expCompTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			model.ExperianDirectors = model.PersonalInfoModel.ExperianDirectors;
			sw.Stop();
			var expDirTime = sw.Elapsed.TotalMilliseconds;
			sw.Restart();
			model.State = "Ok";

			model.MarketplacesHistory = _marketPlaces.GetMarketPlaceHistoryModel(customer).ToList();
			sw.Stop();
			var mpHistTime = sw.Elapsed.TotalMilliseconds;
			totalSw.Stop();

			Log.DebugFormat(@"Customer Id {20}
							Total FullCustomerModel Time taken: {19}ms
							PersonalInfoModel Time taken: {0}ms
							ApplicationInfoModel Time taken: {1}ms
							MarketPlaces and Affordability Time taken: {2}ms
							LoansAndOffers Time taken: {3}ms
							CreditBureauModel Time taken: {4}ms
							SummaryModel Time taken: {5}ms
							PaymentAccountModel Time taken: {6}ms
							MedalCalculations Time taken: {7}ms
							PricingModelCalculations Time taken: {8}ms
							ZooplaAndMortgagesAndPropertiesModel Time taken: {9}ms
							FraudDetectionLog Time taken: {10}ms
							ApiCheckLogs Time taken: {11}ms
							Messages Time taken: {12}ms
							CustomerRelations Time taken: {13}ms
							AlertDocs Time taken: {14}ms
							Bugs Time taken: {15}ms
							CompanyScore Time taken: {16}ms
							ExperianDirectors Time taken: {17}ms
							MarketplacesHistory Time taken: {18}ms", 
							personalTime, appTime, mpTime, loanTime, expTime, summaryTime, paymentTime, medalTime, pricingTime, 
							propTime,fraudTime, apiTime, messagesTime, crmTime, docsTime, bugsTime, expCompTime,
							expDirTime, mpHistTime, totalSw.Elapsed.TotalMilliseconds, customer.Id);
			Log.Debug("full customer model build end");
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		private class FullCustomerModel
		{
			public PersonalInfoModel PersonalInfoModel { get; set; }
			public ApplicationInfoModel ApplicationInfoModel { get; set; }
			public List<MarketPlaceModel> MarketPlaces { get; set; }
			public List<AffordabilityData> Affordability { get; set; }
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
			public List<string> ExperianDirectors { get; set; }
		}
	}
}
