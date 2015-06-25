namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.Linq;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using EzBob.Web.Models;
	using Infrastructure;

	public class PacnetStatusController : Controller
	{
		private readonly IEzbobWorkplaceContext _context;

		public PacnetStatusController(IEzbobWorkplaceContext context)
		{
			_context = context;
		}

		public ActionResult Index()
		{
			var customer = _context.Customer;
			var loan = customer.Loans.Last();
			ViewData["LoanId"] = loan.Id;
			ViewData["Amount"] = FormattingUtils.FormatPounds(loan.LoanAmount);
			ViewData["bankNumber"] = customer.BankAccount.AccountNumber;
			ViewData["schedule"] = loan.Schedule.ToModel();
			ViewData["name"] = customer.PersonalInfo.FirstName + " " + customer.PersonalInfo.Surname;
			ViewData["email"] = customer.Name;
			ViewData["loanNo"] = loan.RefNumber;
			ViewData["SetupFee"] = loan.SetupFee > 0 ? FormattingUtils.FormatPounds(loan.SetupFee) : "";
			ViewData["Total"] = FormattingUtils.FormatPounds(loan.Balance + loan.SetupFee);
			ViewData["IsAlibaba"] = customer.IsAlibaba;

			ViewData["Term"] = loan.CustomerSelectedTerm;
			ViewData["Gender"] = customer.PersonalInfo.GenderName;
			ViewData["IndustryType"] = customer.PersonalInfo.IndustryTypeDescription;
			ViewData["Age"] = customer.PersonalInfo.Age;
			ViewData["TypeOfBusiness"] = customer.PersonalInfo.TypeOfBusinessDescription;

			var addr = customer.AddressInfo.PersonalAddress.FirstOrDefault();
			ViewData["Postcode"] = addr == null ? null : addr.Postcode;
			ViewData["LeadID"] = customer.RefNumber;
			return View("Index");
		}

		public ViewResult TradeTrackerConversion()
		{
			return View();
		}
	}
}
