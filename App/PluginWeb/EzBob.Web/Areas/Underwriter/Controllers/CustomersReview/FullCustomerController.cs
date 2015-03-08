namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using EZBob.DatabaseLib.Model.Database.Loans;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Utils;
	using Models;
	using Newtonsoft.Json;
	using Web.Models;
	using NHibernate;
	using System;
	using System.Text;
	using log4net;

	public class FullCustomerController : Controller {

		public FullCustomerController(
			ICustomerRepository customers,
			ISession session,
			ApplicationInfoModelBuilder infoModelBuilder,
			WebMarketPlacesFacade marketPlaces,
			CreditBureauModelBuilder creditBureauModelBuilder,
			ProfileSummaryModelBuilder summaryModelBuilder,
			CustomerRelationsRepository customerRelationsRepository,
			NHibernateRepositoryBase<MP_AlertDocument> docRepo,
			IBugRepository bugs,
			LoanRepository loanRepository,
			PropertiesModelBuilder propertiesModelBuilder) {
			_customers = customers;
			_session = session;
			_infoModelBuilder = infoModelBuilder;
			_marketPlaces = marketPlaces;
			_creditBureauModelBuilder = creditBureauModelBuilder;
			_summaryModelBuilder = summaryModelBuilder;
			_customerRelationsRepository = customerRelationsRepository;
			_docRepo = docRepo;
			_bugs = bugs;
			_loanRepository = loanRepository;
			this.customerAddressRepository = customerAddressRepository;
			this.landRegistryRepository = landRegistryRepository;
			_propertiesModelBuilder = propertiesModelBuilder;

		} // constructor

		[HttpGet]
		public JsonResult Index(int id) {
			Log.DebugFormat("Build full customer model begin for customer {0}", id);

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
					pi.InitFromCustomer(customer);
					model.PersonalInfoModel = pi;
				} // using

				using (tc.AddStep("ApplicationInfoModel Time taken")) {
					var m = new ApplicationInfoModel();
					_infoModelBuilder.InitApplicationInfo(m, customer, cr);
					model.ApplicationInfoModel = m;
				} // using

				using (tc.AddStep("CreditBureauModel Time taken"))
					model.CreditBureauModel = _creditBureauModelBuilder.Create(customer);

				using (tc.AddStep("SummaryModel Time taken"))
					model.SummaryModel = _summaryModelBuilder.CreateProfile(customer, model.CreditBureauModel);

				using (tc.AddStep("MedalCalculations Time taken"))
					model.MedalCalculations = new MedalCalculators(customer);

				using (tc.AddStep("PropertiesModel Time taken")) {
					model.Properties = _propertiesModelBuilder.Create(customer);
				}
				
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

					DateTime? companySeniority = model.CompanyScore.DashboardModel.OriginationDate;
					int companySeniorityYears = 0, companySeniorityMonths = 0;
					if (companySeniority.HasValue) {
						MiscUtils.GetFullYearsAndMonths(companySeniority.Value, out companySeniorityYears, out companySeniorityMonths);
					}

					model.PersonalInfoModel.CompanySeniority = string.Format("{0}y {1}m", companySeniorityYears, companySeniorityMonths);
					model.PersonalInfoModel.IsYoungCompany = companySeniority.HasValue && companySeniority.Value.AddYears(1) > DateTime.UtcNow && (companySeniority.Value.Year != DateTime.UtcNow.Year || companySeniority.Value.Month != DateTime.UtcNow.Month || companySeniority.Value.Day != DateTime.UtcNow.Day);
				} // using

				using (tc.AddStep("ExperianDirectors Time taken")) {
					var expDirModel = CrossCheckModel.GetExperianDirectors(customer);
					model.ExperianDirectors = expDirModel.DirectorNames;
					model.PersonalInfoModel.NumOfDirectors = expDirModel.NumOfDirectors;
					model.PersonalInfoModel.NumOfShareholders = expDirModel.NumOfShareHolders;
				}

				using (tc.AddStep("MarketplacesHistory Time taken")) {
					model.State = "Ok";
					model.MarketplacesHistory = _marketPlaces.GetMarketPlaceHistoryModel(customer).ToList();
				} // using
			} // using "Total" step

			WriteToLog(tc);
			
			Log.DebugFormat("Build full customer model end for customer {0}", id);

			return Json(model, JsonRequestBehavior.AllowGet);
		} // Index

		private void WriteToLog(TimeCounter tc) {
			var sb = new StringBuilder();
			sb.AppendLine(tc.Title);
			foreach (var time in tc.Steps)
				sb.AppendFormat("\t{0}: {1}ms\n", time.Name, time.Length);

			Log.InfoFormat("{0}", sb);
		}
		
		private readonly ICustomerRepository _customers;
		private readonly ISession _session;
		private readonly ApplicationInfoModelBuilder _infoModelBuilder;
		private readonly WebMarketPlacesFacade _marketPlaces;
		private readonly CreditBureauModelBuilder _creditBureauModelBuilder;
		private readonly ProfileSummaryModelBuilder _summaryModelBuilder;
		private readonly CustomerRelationsRepository _customerRelationsRepository;
		private readonly PropertiesModelBuilder _propertiesModelBuilder;
		private readonly LoanRepository _loanRepository;
		private readonly CustomerAddressRepository customerAddressRepository;
		private readonly LandRegistryRepository landRegistryRepository;
		private readonly NHibernateRepositoryBase<MP_AlertDocument> _docRepo;
		private readonly IBugRepository _bugs;
		private static readonly ILog Log = LogManager.GetLogger(typeof(FullCustomerController));

	} // class FullCustomerController
} // namespace
