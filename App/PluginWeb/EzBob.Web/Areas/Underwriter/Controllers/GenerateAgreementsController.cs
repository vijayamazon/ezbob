namespace EzBob.Web.Areas.Underwriter.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Ezbob.Backend.CalculateLoan.LoanCalculator;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using EzBob.Web.Code;
    using EzBob.Web.Infrastructure;
    using EzBob.Web.Infrastructure.Attributes;
    using EZBob.DatabaseLib.Model.Database.Loans;
    using EZBob.DatabaseLib.Model.Database.Repository;
    using Newtonsoft.Json;

    public class GenerateAgreementsController : Controller
	{
        private readonly IEzbobWorkplaceContext _context;
		private readonly AgreementsGenerator _agreementsGenerator;
		private readonly ILoanRepository _loanRepository;
		private readonly IConcentAgreementHelper _concentAgreementHelper;
		private readonly CustomerRepository _customersRepository;

		public GenerateAgreementsController(AgreementsGenerator agreementsGenerator,
			ILoanRepository loanRepository,
			CustomerRepository customersRepository)
		{
			_agreementsGenerator = agreementsGenerator;
			_loanRepository = loanRepository;
			_concentAgreementHelper = new ConcentAgreementHelper();
			_customersRepository = customersRepository;
		}

		public ViewResult Index()
		{
			return View();
		}

		[HttpPost]
		[Transactional]
		public ActionResult Generate()
		{
			var loans = _loanRepository.GetLoansWithoutAgreements();

			foreach (var loan in loans)
			{
				//NL_Model nlModel = new NL_Model(loan.Customer.Id);
				//nlModel.UserID = this._context.UserId;
				////TODO if relevant history exists use it
				//nlModel.Histories.Add(new NL_LoanHistory() {
				//	Amount = loan.LoanAmount,
				//	EventTime = DateTime.Now // former IssuedTime
				//});

				_agreementsGenerator.RenderAgreements(loan, true/*, nlModel*/);
			}

			return View("Index");
		}

		[Transactional]
		[NoCache]
		public RedirectToRouteResult ReloadAgreementsModel()
		{
			var agreementsModelBuilder = new AgreementsModelBuilder(_context);
			var loans = _loanRepository.GetAll();

			foreach (var loan in loans)
			{
				var agreement = agreementsModelBuilder.ReBuild(loan.Customer, loan);
				loan.AgreementModel = JsonConvert.SerializeObject(agreement);
			}
			return RedirectToAction("Index", "GenerateAgreements", new { Area = "Underwriter" });
		}

		[Transactional]
		[NoCache]
		public RedirectToRouteResult GenerateConsentAgreement()
		{
			var customers = _customersRepository.GetAll().Where(x => x.WizardStep.TheLastOne).ToList();

			foreach (var customer in customers.Where(customer => customer.PersonalInfo != null))
			{
				_concentAgreementHelper.Save(customer, customer.GreetingMailSentDate ?? DateTime.UtcNow);
			}

			return RedirectToAction("Index", "GenerateAgreements", new { Area = "Underwriter" });
		}
	}
}
