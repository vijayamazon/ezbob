namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Code;
	using Code.ReportGenerator;
	using Infrastructure.Attributes;
	using PaymentServices.Calculators;
	using Web.Models;

	public class ScheduleController : Controller
	{
		private readonly LoanBuilder _loanBuilder;
		private readonly ICashRequestRepository _cashRequests;
		private readonly APRCalculator _aprCalc;
		private readonly ICustomerRepository _customerRepository;

		public ScheduleController(LoanBuilder loanBuilder, ICashRequestRepository cashRequests, ICustomerRepository customerRepository)
		{
			_loanBuilder = loanBuilder;
			_cashRequests = cashRequests;
			_aprCalc = new APRCalculator();
			_customerRepository = customerRepository;
		}

		[HttpGet]
		[Transactional]
		[Ajax]
		public JsonResult Calculate(long id)
		{
			var loanOffer = GetLoanOffer(id);
			return Json(loanOffer, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Export(long id, bool isExcel, bool isShowDetails, int customerId)
		{
			var loanOffer = GetLoanOffer(id);
			var customer = _customerRepository.Get(customerId);
			return new LoanOfferReportResult(loanOffer, isExcel, isShowDetails, customer);
		}

		private LoanOffer GetLoanOffer(long id)
		{
			var cr = _cashRequests.Get(id);

			var loan = _loanBuilder.CreateNewLoan(cr, 
				cr.ApprovedSum(), 
				cr.OfferStart.HasValue? cr.OfferStart.Value : DateTime.UtcNow, 
				cr.ApprovedRepaymentPeriod.HasValue ? cr.ApprovedRepaymentPeriod.Value : 12);

			var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date);
			calc.GetState();

			var apr = loan.LoanAmount == 0 ? 0 : _aprCalc.Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date);

			var loanOffer = LoanOffer.InitFromLoan(loan, apr, null, cr);

			return loanOffer;
		}
	}
}
