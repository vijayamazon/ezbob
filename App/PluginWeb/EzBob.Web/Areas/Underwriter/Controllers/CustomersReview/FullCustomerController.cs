using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.CustomerRelations;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.Models;
using EzBob.Models.Marketplaces;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Models;
using NHibernate;
using Scorto.Web;
using System;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class FullCustomerController : Controller
	{

		private readonly ICustomerRepository _customers;
		private readonly ISession _session;
		private readonly ApplicationInfoModelBuilder _infoModelBuilder;
		private readonly MarketPlacesFacade _marketPlaces;
		private readonly CreditBureauModelBuilder _creditBureauModelBuilder;
		private readonly ProfileSummaryModelBuilder _summaryModelBuilder;
		private readonly FraudDetectionRepository _fraudDetectionLog;
		private readonly ApiCheckLogBuilder _apiCheckLogBuilder;
		private readonly MessagesModelBuilder _messagesModelBuilder;
		private readonly CustomerRelationsRepository _customerRelationsRepository;
		private readonly LoanRepository _loanRepository;
		private readonly NHibernateRepositoryBase<MP_AlertDocument> _docRepo;
		private readonly IBugRepository _bugs;

		public FullCustomerController(
											ICustomerRepository customers,
											ISession session,
											ApplicationInfoModelBuilder infoModelBuilder,
											MarketPlacesFacade marketPlaces,
											CreditBureauModelBuilder creditBureauModelBuilder,
											ProfileSummaryModelBuilder summaryModelBuilder,
											FraudDetectionRepository fraudDetectionLog,
											ApiCheckLogBuilder apiCheckLogBuilder,
											MessagesModelBuilder messagesModelBuilder,
											CustomerRelationsRepository customerRelationsRepository,
											NHibernateRepositoryBase<MP_AlertDocument> docRepo,
											IBugRepository bugs, 
											LoanRepository loanRepository)
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
		}

		//[Ajax]
		[Transactional]
		[HttpGet]
		public JsonNetResult Index(int id, string history = null)
		{
			var model = new FullCustomerModel();

			var customer = _customers.TryGet(id);

			if (customer == null)
			{
				model.State = "NotFound";
				return this.JsonNet(model);
			}

			if (!customer.WizardStep.TheLastOne) {
				model.State = "NotSuccesfullyRegistred";
				return this.JsonNet(model);
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

			DateTime? lastDateCheck = null;
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

			var crm = new CustomerRelationsModelBuilder(_loanRepository, _customerRelationsRepository);

			model.CustomerRelations = crm.Create(customer.Id).ToList();

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
			return this.JsonNet(model);
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
			public FraudDetectionLogModel FraudDetectionLog { get; set; }
			public List<ApiChecksLogModel> ApiCheckLogs { get; set; }
			public List<MessagesModel> Messages { get; set; }
			public List<CustomerRelationsModel> CustomerRelations { get; set; }
			public AlertDoc[] AlertDocs { get; set; }
			public List<BugModel> Bugs { get; set; }
			public string State { get; set; }
			public CompanyScoreModel CompanyScore { get; set; }
			public SortedDictionary<string, int> ExperianDirectors { get; set; }
		}

	}
}
