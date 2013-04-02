using System.Linq;
using System.Web.Mvc;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using Scorto.Web;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class    PacnetStatusController : Controller
    {
        private readonly IEzbobWorkplaceContext _context;

        public PacnetStatusController(IEzbobWorkplaceContext context)
        {
            _context = context;
        }

        [Transactional]
        public ActionResult Index()
        {
            var amount = (decimal)TempData["amount"];
            var bankNumber = (string)TempData["bankNumber"];
            var card_no = TempData["card_no"];

            var customer = _context.Customer;
            var loan = customer.Loans.Last();

            ViewData["Amount"] = FormattingUtils.FormatPounds(amount);
            ViewData["LoanId"] = loan.Id;

            ViewData["bankNumber"] = bankNumber;
            ViewData["card_no"] = card_no;
            ViewData["schedule"] = loan.Schedule.ToModel();
            ViewData["name"] = customer.PersonalInfo.FirstName + " " + customer.PersonalInfo.Surname;
            ViewData["email"] = customer.Name;
            
            ViewData["loanNo"] = loan.RefNumber;
            ViewData["SetupFee"] = loan.SetupFee > 0 ? FormattingUtils.FormatPounds(loan.SetupFee) : "";
            ViewData["Total"] = FormattingUtils.FormatPounds(loan.Balance + loan.SetupFee);
            return View("Index" );
        }
    }
}
