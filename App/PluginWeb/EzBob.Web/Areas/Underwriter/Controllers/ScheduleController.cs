namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Code;
	using Code.ReportGenerator;
	using ConfigManager;
	using EzServiceAccessor;
	using Infrastructure.Attributes;
	using log4net;
	using PaymentServices.Calculators;
	using StructureMap;
	using Web.Models;

	public class ScheduleController : Controller
	{
		private readonly LoanBuilder _loanBuilder;
		private readonly ICashRequestRepository _cashRequests;
		private readonly APRCalculator _aprCalc;
		private readonly ICustomerRepository _customerRepository;
	    private static readonly ILog Log = LogManager.GetLogger(typeof (ScheduleController));

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

			var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date, CurrentValues.Instance.AmountToChargeFrom);
			calc.GetState();

            try {
                long nl_LoanId = ObjectFactory.GetInstance<IEzServiceAccessor>().GetLoanByOldID(loan.Id, 1, 1);
                var nlModel = ObjectFactory.GetInstance<IEzServiceAccessor>().GetLoanState(loan.Customer.Id, nl_LoanId, DateTime.UtcNow, 1, true);
                Log.Info(string.Format("<<< NL_Compare at : {0} ;  New : {1} Old: {2} >>>", System.Environment.StackTrace, loan, nlModel));
            }
            catch (Exception) {
                Log.Info(string.Format("<<< NL_Compare Fail at : {0}", System.Environment.StackTrace));
            }

			var apr = loan.LoanAmount == 0 ? 0 : _aprCalc.Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date);

			var loanOffer = LoanOffer.InitFromLoan(loan, apr, null, cr);
            
            
            //TODO calculate offer
            Log.DebugFormat("calculate offer for customer {0}", cr.Customer.Id);

			return loanOffer;
		}
	}
}
