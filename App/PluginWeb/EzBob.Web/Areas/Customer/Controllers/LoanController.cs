namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Models;
	using Infrastructure.Attributes;
	using Code;
	using ConfigManager;
	using Infrastructure;
	using Infrastructure.csrf;
	using PaymentServices;
	using PaymentServices.Calculators;

	public class LoanController : Controller {
		private readonly IEzbobWorkplaceContext _context;
		private readonly PaymentRolloverRepository _rolloverRepository;

		public LoanController(IEzbobWorkplaceContext context, PaymentRolloverRepository rolloverRepository) {
			_context = context;
			_rolloverRepository = rolloverRepository;
		}

		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Ajax]
		public JsonResult Details(int id) {
			var customer = _context.Customer;

			var loan = customer.Loans.SingleOrDefault(l => l.Id == id);

			if (loan == null) {
				return Json(new { error = "loan does not exists" }, JsonRequestBehavior.AllowGet);
			}

			var loansDetailsBuilder = new LoansDetailsBuilder();
			var details = loansDetailsBuilder.Build(loan, _rolloverRepository.GetByLoanId(loan.Id));

			return Json(details, JsonRequestBehavior.AllowGet);
		}

		[ValidateJsonAntiForgeryToken]
		[HttpGet]
		[Ajax]
		public JsonResult Get(int id) {
			var customer = _context.Customer;
			var loan = customer.Loans.SingleOrDefault(l => l.Id == id);

			ILoanRepaymentScheduleCalculator calculator = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow, CurrentValues.Instance.AmountToChargeFrom);

			var loanModel = LoanModel.FromLoan(loan, calculator);

			return Json(loanModel, JsonRequestBehavior.AllowGet);

		}
	}
}
