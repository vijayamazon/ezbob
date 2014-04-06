namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure.Attributes;
	using PaymentServices.Calculators;
	using log4net;

    public class AdminController : Controller
    {

        private static readonly ILog log = LogManager.GetLogger("Underwriter.AdminController");

        private readonly ICustomerRepository _customers;
        private readonly ILoanRepository _loans;

        public AdminController(ICustomerRepository customers, ILoanRepository loans)
        {
            _customers = customers;
            _loans = loans;
        }

        [Transactional]
        public ViewResult Index()
        {
            return View();
        }

        [Transactional]
        public RedirectToRouteResult Regenerate()
        {
            var loans = _loans.LiveLoans();

            foreach (var loan in loans)
            {
                RegenerateLoan(loan);
            }

            return RedirectToAction("Index", new{area="Underwriter"});
        }

        private void RegenerateLoan(Loan loan)
        {
            log.InfoFormat("Recalculating loan:{1}{0}", loan, Environment.NewLine);
            var c = new LoanScheduleCalculator(){Interest = loan.InterestRate, SetUpFee = loan.SetupFee, Term = loan.Schedule.Count};
            c.Calculate(loan.LoanAmount, loan, loan.Date);
            log.InfoFormat("Done:{1}{0}", loan, Environment.NewLine);
        }
    }
}