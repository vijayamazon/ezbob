namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EzBob.Web.Code;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Newtonsoft.Json;

	public class GenerateAgreementsController : Controller {
		public GenerateAgreementsController(
			IEzbobWorkplaceContext context,
			AgreementsGenerator agreementsGenerator,
			ILoanRepository loanRepository,
			CustomerRepository customersRepository,
			ILoanLegalRepository llrepo
		) {
			this.context = context;
			this.loanLegalRepo = llrepo;
			this.agreementsGenerator = agreementsGenerator;
			this.loanRepository = loanRepository;
			this.concentAgreementHelper = new ConcentAgreementHelper();
			this.customersRepository = customersRepository;
		} // constructor

		public ViewResult Index() {
			return View();
		} // Index

		[HttpPost]
		[Transactional]
		public ActionResult Generate() {
			var loans = this.loanRepository.GetLoansWithoutAgreements();

			foreach (var loan in loans) {
				//NL_Model nlModel = new NL_Model(loan.Customer.Id);
				//nlModel.UserID = this.context.UserId;
				////TODO if relevant history exists use it
				//nlModel.Histories.Add(new NL_LoanHistory() {
				//	Amount = loan.LoanAmount,
				//	EventTime = DateTime.Now // former IssuedTime
				//});

				this.agreementsGenerator.RenderAgreements(loan, true/*, nlModel*/);
			} // for each

			return View("Index");
		} // Generate

		[Transactional]
		[NoCache]
		public RedirectToRouteResult ReloadAgreementsModel() {
			var agreementsModelBuilder = new AgreementsModelBuilder(this.loanLegalRepo, this.context);
			var loans = this.loanRepository.GetAll();

			foreach (var loan in loans) {
				var agreement = agreementsModelBuilder.ReBuild(loan.Customer, loan);
				loan.AgreementModel = JsonConvert.SerializeObject(agreement);
			} // for each

			return RedirectToAction("Index", "GenerateAgreements", new { Area = "Underwriter" });
		} // ReloadAgreementsModel

		[Transactional]
		[NoCache]
		public RedirectToRouteResult GenerateConsentAgreement() {
			var customers = this.customersRepository.GetAll().Where(x => x.WizardStep.TheLastOne).ToList();

			foreach (var customer in customers.Where(customer => customer.PersonalInfo != null))
				this.concentAgreementHelper.Save(customer, customer.GreetingMailSentDate ?? DateTime.UtcNow);

			return RedirectToAction("Index", "GenerateAgreements", new { Area = "Underwriter" });
		} // GenerateConsentAgreement

		private readonly IEzbobWorkplaceContext context;
		private readonly ILoanLegalRepository loanLegalRepo;
		private readonly AgreementsGenerator agreementsGenerator;
		private readonly ILoanRepository loanRepository;
		private readonly IConcentAgreementHelper concentAgreementHelper;
		private readonly CustomerRepository customersRepository;
	} // class GenerateAgreementsController
} // namespace
