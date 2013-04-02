using System;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Code;
using PaymentServices.Calculators;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly LoanBuilder _loanBuilder;
        private readonly ICashRequestRepository _cashRequests;
        private readonly APRCalculator _aprCalc;

        public ScheduleController(LoanBuilder loanBuilder, ICashRequestRepository cashRequests)
        {
            _loanBuilder = loanBuilder;
            _cashRequests = cashRequests;
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
            var total = loan.Schedule.Sum(s => s.AmountDue) + loan.SetupFee;
            var totalPrincipal = loan.Schedule.Sum(s => s.LoanRepayment);
            var totalInterest = loan.Schedule.Sum(s => s.Interest) + loan.Charges.Sum(x => x.Amount) + loan.SetupFee;
            var realInterestCost = loan.LoanAmount == 0 ? 0 : totalInterest / loan.LoanAmount;
            var timestamp = DateTime.UtcNow.Ticks;


            return this.JsonNet(new
                                    {
                                        schedule = loan.Schedule.Select(s => LoanScheduleItemModel.FromLoanScheduleItem(s)).ToArray(),
                                        apr,
                                        loan.SetupFee,
                                        total,
                                        realInterestCost,
                                        loan.LoanAmount,
                                        timestamp,
                                        totalInterest,
                                        totalPrincipal
                                    });
        }

    }
}