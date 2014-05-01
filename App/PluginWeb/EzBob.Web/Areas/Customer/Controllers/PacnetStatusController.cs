namespace EzBob.Web.Areas.Customer.Controllers
{
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using Ezbob.Backend.Models;
	using Infrastructure.Attributes;
	using Models;
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


			List<LoanScheduleItemModel> l = new List<LoanScheduleItemModel>();
			foreach (var s in loan.Schedule)
			{
				LoanScheduleItemModel n = new LoanScheduleItemModel
				{
					Id = s.Id,
					AmountDue = s.AmountDue,
					Date = s.Date,
					PrevInstallmentDate = s.PrevInstallmentDate,
					Interest = s.Interest,
					InterestPaid = s.InterestPaid,
					LateCharges = s.LateCharges,
					RepaymentAmount = s.RepaymentAmount,
					Status = s.Status.ToString(),
					StatusDescription = s.Status.ToDescription(),
					LoanRepayment = s.LoanRepayment,
					Balance = s.Balance,
					BalanceBeforeRepayment = s.BalanceBeforeRepayment,
					Fees = s.Fees,
					InterestRate = s.InterestRate
				};

				l.Add(n);
			}

			ViewData["schedule"] = l;
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
