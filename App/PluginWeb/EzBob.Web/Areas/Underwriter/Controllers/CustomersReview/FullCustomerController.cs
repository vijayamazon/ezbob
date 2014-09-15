namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System.Globalization;
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
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Infrastructure;
	using Models;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using Web.Models;
	using NHibernate;
	using System;

	public class FullCustomerController : Controller {
		#region constructor

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
			LandRegistryRepository landRegistryRepository, 
			PropertiesModelBuilder propertiesModelBuilder,
			IWorkplaceContext context) {
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
			_propertiesModelBuilder = propertiesModelBuilder;
			_context = context;
			serviceClient = new ServiceClient();

		} // constructor

		#endregion constructor

		#region action Index

		[HttpGet]
		public JsonResult Index(int id, string history = null) {
			Log.Debug("Build full customer model begin for customer {0} and history = {1}.", id, history);

			var model = new FullCustomerModel();

			var customer = _customers.TryGet(id);

			if (customer == null) {
				model.State = "NotFound";
				return Json(model, JsonRequestBehavior.AllowGet);
			} // if

			TimeCounter tc = new TimeCounter("FullCustomerModel building time for customer " + customer.Stringify());

			using (tc.AddStep("Customer {0} total FullCustomerModel time taken", customer.Stringify())) {
				var cr = customer.LastCashRequest;

				using (tc.AddStep("PersonalInfoModel Time taken")) {
					var pi = new PersonalInfoModel();
					pi.InitFromCustomer(customer, _session);
					model.PersonalInfoModel = pi;
				} // using

				using (tc.AddStep("ApplicationInfoModel Time taken")) {
					var m = new ApplicationInfoModel();
					_infoModelBuilder.InitApplicationInfo(m, customer, cr);
					model.ApplicationInfoModel = m;
				} // using

				using (tc.AddStep("MarketPlaces and Affordability Time taken")) {
					DateTime historyDate;
					bool bHasHistoryDate = DateTime.TryParseExact(history, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out historyDate);

					var ar = serviceClient.Instance.CalculateModelsAndAffordability(_context.UserId, id, bHasHistoryDate ? historyDate : (DateTime?)null);

					model.MarketPlaces = JsonConvert.DeserializeObject<MarketPlaceModel[]>(ar.Models).ToList();
					model.Affordability = ar.Affordability.ToList();
				} // using

				using (tc.AddStep("LoansAndOffers Time taken"))
					model.LoansAndOffers = new LoansAndOffers(customer);

				using (tc.AddStep("CreditBureauModel Time taken"))
					model.CreditBureauModel = _creditBureauModelBuilder.Create(customer);

				using (tc.AddStep("SummaryModel Time taken"))
					model.SummaryModel = _summaryModelBuilder.CreateProfile(customer, model.CreditBureauModel);

				using (tc.AddStep("PaymentAccountModel Time taken"))
					model.PaymentAccountModel = new PaymentsAccountModel(customer);

				using (tc.AddStep("MedalCalculations Time taken"))
					model.MedalCalculations = new MedalCalculators(customer);

				using (tc.AddStep("PricingModelCalculations Time taken")) {
					try {
						PricingModelModelActionResult getPricingModelModelResponse =
							serviceClient.Instance.GetPricingModelModel(customer.Id, _context.UserId, "Basic");

						model.PricingModelCalculations = getPricingModelModelResponse.Value;
					}
					catch (Exception ex) {
						Log.Error(ex, "Failed to load pricing model.");
					}
				} // using

				using (tc.AddStep("PropertiesModel Time taken")) {
					model.Properties = _propertiesModelBuilder.Create(customer);
				}
				using (tc.AddStep("FraudDetectionLog Time taken")) {
					DateTime? lastDateCheck;

					model.FraudDetectionLog = new FraudDetectionLogModel {
						FraudDetectionLogRows = _fraudDetectionLog.GetLastDetections(id, out lastDateCheck)
							.Select(x => new FraudDetectionLogRowModel(x))
							.OrderByDescending(x => x.Id)
							.ToList(),
						LastCheckDate = lastDateCheck,
					};
				} // using

				using (tc.AddStep("ApiCheckLogs Time taken"))
					model.ApiCheckLogs = _apiCheckLogBuilder.Create(customer);

				using (tc.AddStep("Messages Time taken"))
					model.Messages = _messagesModelBuilder.Create(customer);

				using (tc.AddStep("CustomerRelations Time taken")) {
					var crm = new CustomerRelationsModelBuilder(_loanRepository, _customerRelationsRepository, _session);
					model.CustomerRelations = crm.Create(customer.Id);
				} // using

				using (tc.AddStep("AlertDocs Time taken"))
					model.AlertDocs = (from d in _docRepo.GetAll() where d.Customer.Id == id select AlertDoc.FromDoc(d)).ToArray();

				using (tc.AddStep("Bugs Time taken"))
					model.Bugs = _bugs.GetAll().Where(x => x.Customer.Id == customer.Id).Select(x => BugModel.ToModel(x)).ToList();

				using (tc.AddStep("CompanyScore Time taken")) {
					var builder = new CompanyScoreModelBuilder();
					model.CompanyScore = builder.Create(customer);
				} // using

				using (tc.AddStep("ExperianDirectors Time taken"))
					model.ExperianDirectors = model.PersonalInfoModel.ExperianDirectors;

				using (tc.AddStep("MarketplacesHistory Time taken")) {
					model.State = "Ok";
					model.MarketplacesHistory = _marketPlaces.GetMarketPlaceHistoryModel(customer).ToList();
				} // using
			} // using "Total" step

			tc.Log(Log);

			Log.Debug("Build full customer model end for customer {0} and history = {1}.", id, history);

			return Json(model, JsonRequestBehavior.AllowGet);
		} // Index

		#endregion action Index

		#region private

		#region class FullCustomerModel
		// ReSharper disable UnusedAutoPropertyAccessor.Local

		private class FullCustomerModel {
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
		} // class FullCustomerModel

		// ReSharper restore UnusedAutoPropertyAccessor.Local
		#endregion class FullCustomerModel

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
		private readonly PropertiesModelBuilder _propertiesModelBuilder;
		private readonly LoanRepository _loanRepository;
		private readonly CustomerAddressRepository customerAddressRepository;
		private readonly LandRegistryRepository landRegistryRepository;
		private readonly NHibernateRepositoryBase<MP_AlertDocument> _docRepo;
		private readonly IBugRepository _bugs;
		private readonly ServiceClient serviceClient;
		private readonly IWorkplaceContext _context;
		private static readonly ASafeLog Log = new SafeILog(typeof(FullCustomerController));

		#endregion private
	} // class FullCustomerController
} // namespace
