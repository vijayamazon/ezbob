using System;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
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
        private readonly ICustomerRepository _customerRepository;

        public ScheduleController(LoanBuilder loanBuilder, ICashRequestRepository cashRequests, ILoanTypeRepository loanType, ICustomerRepository customerRepository)
        {
            _loanBuilder = loanBuilder;
            _cashRequests = cashRequests;
            _loanTypes = loanType;
            _aprCalc = new APRCalculator();
            _customerRepository = customerRepository;
        }

        [HttpGet]
        [Transactional]
        [Ajax]
        public JsonNetResult Calculate(long id)
        {
            var loanOffer = GetLoanOffer(id);
            return this.JsonNet(loanOffer);
        }

        [Transactional]
        public ActionResult Export(long id, bool isExcel, bool isShowDetails, int customerId)
        {
            var loanOffer = GetLoanOffer(id);
            var customer = _customerRepository.Get(customerId);
            return new LoanOfferReportResult(loanOffer, isExcel, isShowDetails, customer);
        }

        private LoanOffer GetLoanOffer(long id)
        {
            var cr = _cashRequests.Get(id);
            var loan = _loanBuilder.CreateLoan(cr, cr.ApprovedSum(), cr.Customer.OfferStart.Value);
            
            var calc = new LoanRepaymentScheduleCalculator(loan, loan.Date);
            calc.GetState();
 
            var apr = loan.LoanAmount == 0 ? 0 : _aprCalc.Calculate(loan.LoanAmount, loan.Schedule, loan.SetupFee, loan.Date);

            var loanOffer = LoanOffer.InitFromLoan(loan, apr, null, cr);

            return loanOffer;
        }
    }
}