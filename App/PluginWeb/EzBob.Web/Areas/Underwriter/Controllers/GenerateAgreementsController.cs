﻿namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System.Data;
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Code;
	using Code.Agreements;
	using Infrastructure.Attributes;
	using Newtonsoft.Json;

	public class GenerateAgreementsController : Controller
    {
        private readonly AgreementsGenerator _agreementsGenerator;
        private readonly ILoanRepository _loanRepository;
        private readonly IConcentAgreementHelper _concentAgreementHelper;
        private readonly CustomerRepository _customersRepository;

        public GenerateAgreementsController(AgreementsGenerator agreementsGenerator, ILoanRepository loanRepository, CustomerRepository customersRepository)
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
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public ActionResult Generate()
        {
            var loans = _loanRepository.GetLoansWithoutAgreements();

            foreach (var loan in loans)
            {
                _agreementsGenerator.RenderAgreements(loan, true);
            }
           
            return View("Index");
        }

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [NoCache]
        public RedirectToRouteResult ReloadAgreementsModel()
        {
            var agreementsModelBuilder = new AgreementsModelBuilder();
            var loans = _loanRepository.GetAll();

            foreach (var loan in loans)
            {
                var agreement = agreementsModelBuilder.ReBuild(loan.Customer, loan);
                loan.AgreementModel = JsonConvert.SerializeObject(agreement);
            }
            return RedirectToAction("Index", "GenerateAgreements", new { Area = "Underwriter" });
        }

        
        [Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [NoCache]
        public RedirectToRouteResult GenerateConsentAgreement()
        {
            var customers = _customersRepository.GetAll().Where(x => x.WizardStep.TheLastOne).ToList() ;

            foreach (var customer in customers.Where(customer => customer.PersonalInfo !=null))
            {
                _concentAgreementHelper.Save(customer, customer.GreetingMailSentDate ?? DateTime.UtcNow);
            }
            
            return RedirectToAction("Index", "GenerateAgreements", new { Area = "Underwriter" });
        }
    }
}