namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using EZBob.DatabaseLib.Model.Database.Loans;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Utils;
	using Models;
	using Web.Models;
	using NHibernate;
	using System;
	using Ezbob.Logger;
	using EzBob.Web.Infrastructure;
	using ServiceClientProxy;

	public class FullCustomerController : Controller {
		public FullCustomerController(
			ICustomerRepository customerRepo,
			ISession session,
			CreditBureauModelBuilder creditBureauModelBuilder,
			ProfileSummaryModelBuilder summaryModelBuilder,
			CustomerRelationsRepository customerRelationsRepo,
			IBugRepository bugRepo,
			LoanRepository loanRepo,
			PropertiesModelBuilder propertiesModelBuilder, 
			IEzbobWorkplaceContext context, 
			ServiceClient serviceClient) {
			this.customerRepo = customerRepo;
			this.session = session;
			this.creditBureauModelBuilder = creditBureauModelBuilder;
			this.summaryModelBuilder = summaryModelBuilder;
			this.customerRelationsRepo = customerRelationsRepo;
			this.bugRepo = bugRepo;
			this.loanRepo = loanRepo;
			this.propertiesModelBuilder = propertiesModelBuilder;
			this.context = context;
			this.serviceClient = serviceClient;
		} // constructor

		[HttpGet]
		public JsonResult Index(int id) {
			log.Debug("Build full customer model begin for customer {0}", id);

			var model = new FullCustomerModel();

			var customer = this.customerRepo.ReallyTryGet(id);

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
					try {
						var aiar = this.serviceClient.Instance.LoadApplicationInfo(
							this.context.UserId,
							customer.Id,
							cr == null ? (long?)null : cr.Id,
							DateTime.UtcNow
							);

						model.ApplicationInfoModel = aiar.Model;
					} catch (Exception ex) {
						log.Error(ex, "Failed to load application info model for customer {0} cr {1}", customer.Id, cr == null ? (long?)null : cr.Id);
					}
				} // using

				using (tc.AddStep("CreditBureauModel Time taken"))
					model.CreditBureauModel = this.creditBureauModelBuilder.Create(customer);

				using (tc.AddStep("CompanyScore Time taken")) {
					var builder = new CompanyScoreModelBuilder();
					model.CompanyScore = builder.Create(customer);

					DateTime? companySeniority = model.CompanyScore.DashboardModel.OriginationDate;
					int companySeniorityYears = 0, companySeniorityMonths = 0;
					if (companySeniority.HasValue) {
						MiscUtils.GetFullYearsAndMonths(
							companySeniority.Value,
							out companySeniorityYears,
							out companySeniorityMonths
						);
					} // if

					model.PersonalInfoModel.CompanySeniority = string.Format(
						"{0}y {1}m",
						companySeniorityYears,
						companySeniorityMonths
					);

					model.PersonalInfoModel.IsYoungCompany =
						companySeniority.HasValue &&
						companySeniority.Value.AddYears(1) > DateTime.UtcNow && (
							companySeniority.Value.Year != DateTime.UtcNow.Year ||
							companySeniority.Value.Month != DateTime.UtcNow.Month ||
							companySeniority.Value.Day != DateTime.UtcNow.Day
						);
				} // using

				using (tc.AddStep("SummaryModel Time taken"))
					model.SummaryModel = this.summaryModelBuilder.CreateProfile(customer, model.CreditBureauModel, model.CompanyScore);

				using (tc.AddStep("MedalCalculations Time taken"))
					model.MedalCalculations = new MedalCalculators(customer);

				using (tc.AddStep("PropertiesModel Time taken"))
					model.Properties = this.propertiesModelBuilder.Create(customer);

				using (tc.AddStep("CustomerRelations Time taken")) {
					var crm = new CustomerRelationsModelBuilder(
						this.loanRepo,
						this.customerRelationsRepo,
						this.session
					);
					model.CustomerRelations = crm.Create(customer.Id);
				} // using

				using (tc.AddStep("Bugs Time taken")) {
					model.Bugs = this.bugRepo
						.GetAll()
						.Where(x => x.Customer.Id == customer.Id)
						.Select(x => BugModel.ToModel(x))
						.ToList();
				} // using



				using (tc.AddStep("ExperianDirectors Time taken")) {
					var expDirModel = CrossCheckModel.GetExperianDirectors(customer);
					model.ExperianDirectors = expDirModel.DirectorNames;
					model.PersonalInfoModel.NumOfDirectors = expDirModel.NumOfDirectors;
					model.PersonalInfoModel.NumOfShareholders = expDirModel.NumOfShareHolders;
				} // using

				model.State = "Ok";
			} // using "Total" step

			log.Info(tc.ToString());

			log.Debug("Build full customer model end for customer {0}", id);

			return Json(model, JsonRequestBehavior.AllowGet);
		} // Index

		private readonly ICustomerRepository customerRepo;
		private readonly ISession session;
		private readonly CreditBureauModelBuilder creditBureauModelBuilder;
		private readonly ProfileSummaryModelBuilder summaryModelBuilder;
		private readonly CustomerRelationsRepository customerRelationsRepo;
		private readonly PropertiesModelBuilder propertiesModelBuilder;
		private readonly LoanRepository loanRepo;
		private readonly IBugRepository bugRepo;
		private readonly IEzbobWorkplaceContext context;
		private readonly ServiceClientProxy.ServiceClient serviceClient;
		private static readonly ASafeLog log = new SafeILog(typeof(FullCustomerController));
	} // class FullCustomerController
} // namespace
