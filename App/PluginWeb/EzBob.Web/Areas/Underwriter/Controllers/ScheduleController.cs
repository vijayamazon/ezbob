using System;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using EzBob.Web.Code;
using EzBob.Web.Code.ReportGenerator;
using PaymentServices.Calculators;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly LoanBuilder _loanBuilder;
        private readonly ICashRequestRepository _cashRequests;
        private readonly APRCalculator _aprCalc;
        private readonly ILoanTypeRepository _loanTypes;
        private readonly RepaymentCalculator _repaymentCalculator = new RepaymentCalculator();

        public ScheduleController(LoanBuilder loanBuilder, ICashRequestRepository cashRequests, ILoanTypeRepository loanType)
        {
            _loanBuilder = loanBuilder;
            _cashRequests = cashRequests;
            _loanTypes = loanType;
            _aprCalc = new APRCalculator();
        }

        [HttpGet]
        [Transactional]
        [Ajax]
        public JsonNetResult Calculate(long id)
        {
            var cr = _cashRequests.Get(id);

            var loan = _loanBuilder.CreateLoan(cr, cr.ApprovedSum(), DateTime.UtcNow);

            var apr = loan.LoanAmount == 0 ? 0 : _aprCalc.Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee);

            var loanOffer = LoanOffer.InitFromLoan(loan, apr, null);
            return this.JsonNet(loanOffer);

        }

        public ActionResult Export(long id, bool isExcel, bool isShowDetails)
        {
            var cr = _cashRequests.Get(id);

            var loan = _loanBuilder.CreateLoan(cr, cr.ApprovedSum(), DateTime.UtcNow);

            var apr = loan.LoanAmount == 0 ? 0 : _aprCalc.Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee);

            var loanOffer = LoanOffer.InitFromLoan(loan, apr, null);

            loanOffer.Details = new LoanOfferDetails
                {
                    InterestRate = cr.InterestRate,
                    RepaymentPeriod = _repaymentCalculator.ReCalculateRepaymentPeriod(cr),
                    OfferedCreditLine = 15,
                    LoanType = _loanTypes.GetAll().First().Name
                };

            return new LoanOfferReportResult(loanOffer, isExcel, isShowDetails);

        }
    }
}