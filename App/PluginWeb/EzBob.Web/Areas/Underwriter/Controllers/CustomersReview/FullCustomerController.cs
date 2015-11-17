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
			this._customers = customers;
            this._session = session;
            this._infoModelBuilder = infoModelBuilder;
            this._marketPlaces = marketPlaces;
            this._creditBureauModelBuilder = creditBureauModelBuilder;
            this._summaryModelBuilder = summaryModelBuilder;
            this._customerRelationsRepository = customerRelationsRepository;
            this._docRepo = docRepo;
            this._bugs = bugs;
            this._loanRepository = loanRepository;
            this._propertiesModelBuilder = propertiesModelBuilder;

		} // constructor

		[HttpGet]
		public JsonResult Index(int id) {
			Log.DebugFormat("Build full customer model begin for customer {0}", id);

			var model = new FullCustomerModel();

            var customer = this._customers.TryGet(id);

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
                    this._infoModelBuilder.InitApplicationInfo(m, customer, cr);
					model.ApplicationInfoModel = m;

					var aiar = new ServiceClientProxy.ServiceClient().Instance.LoadApplicationInfo(
						0,
						customer.Id,
						cr == null ? (long?)null : cr.Id,
						DateTime.UtcNow
					);

					ApplicationInfoController.VerifyApplicationInfoModels(m, aiar.Model);
				} // using

				using (tc.AddStep("CreditBureauModel Time taken"))
                    model.CreditBureauModel = this._creditBureauModelBuilder.Create(customer);

				using (tc.AddStep("SummaryModel Time taken"))
                    model.SummaryModel = this._summaryModelBuilder.CreateProfile(customer, model.CreditBureauModel);

				using (tc.AddStep("MedalCalculations Time taken"))
					model.MedalCalculations = new MedalCalculators(customer);

				using (tc.AddStep("PropertiesModel Time taken")) {
                    model.Properties = this._propertiesModelBuilder.Create(customer);
				}
				
				using (tc.AddStep("CustomerRelations Time taken")) {
                    var crm = new CustomerRelationsModelBuilder(this._loanRepository, this._customerRelationsRepository, this._session);
					model.CustomerRelations = crm.Create(customer.Id);
				} // using

				using (tc.AddStep("Bugs Time taken"))
                    model.Bugs = this._bugs.GetAll().Where(x => x.Customer.Id == customer.Id).Select(x => BugModel.ToModel(x)).ToList();

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

				model.State = "Ok";
			} // using "Total" step

			Log.Info(tc.ToString());
			
			Log.DebugFormat("Build full customer model end for customer {0}", id);

			return Json(model, JsonRequestBehavior.AllowGet);
		} // Index

		
		private readonly ICustomerRepository _customers;
		private readonly ISession _session;
		private readonly ApplicationInfoModelBuilder _infoModelBuilder;
		private readonly WebMarketPlacesFacade _marketPlaces;
		private readonly CreditBureauModelBuilder _creditBureauModelBuilder;
		private readonly ProfileSummaryModelBuilder _summaryModelBuilder;
		private readonly CustomerRelationsRepository _customerRelationsRepository;
		private readonly PropertiesModelBuilder _propertiesModelBuilder;
		private readonly LoanRepository _loanRepository;
		private readonly NHibernateRepositoryBase<MP_AlertDocument> _docRepo;
		private readonly IBugRepository _bugs;
		private static readonly ILog Log = LogManager.GetLogger(typeof(FullCustomerController));

	} // class FullCustomerController
} // namespace
