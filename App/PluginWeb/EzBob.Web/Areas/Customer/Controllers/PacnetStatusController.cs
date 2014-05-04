namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.Data;
	using System.Linq;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using Infrastructure.Attributes;
	using Code;
	using Infrastructure;

    public class PacnetStatusController : Controller
    {
        private readonly IEzbobWorkplaceContext _context;

        public PacnetStatusController(IEzbobWorkplaceContext context)
        {
            _context = context;
        }

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public ActionResult Index()
        {
			var customer = _context.Customer;
			var loan = customer.Loans.Last();
			ViewData["LoanId"] = loan.Id;
			ViewData["Amount"] = FormattingUtils.FormatPounds(_context.Customer.Loans.Last().LoanAmount);
			ViewData["bankNumber"] = customer.BankAccount.AccountNumber;
			ViewData["schedule"] = loan.Schedule.ToModel();
			ViewData["name"] = customer.PersonalInfo.FirstName + " " + customer.PersonalInfo.Surname;
			ViewData["email"] = customer.Name;
			ViewData["loanNo"] = loan.RefNumber;
			ViewData["SetupFee"] = loan.SetupFee > 0 ? FormattingUtils.FormatPounds(loan.SetupFee) : "";
			ViewData["Total"] = FormattingUtils.FormatPounds(loan.Balance + loan.SetupFee);

            return View("Index" );
        }

		public ViewResult TradeTrackerConversion()
		{
			return View();
		}
    }
}
