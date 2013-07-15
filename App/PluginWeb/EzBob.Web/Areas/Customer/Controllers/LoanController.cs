using System;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Repository;
using EzBob.Models;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.csrf;
using PaymentServices.Calculators;
using Scorto.Web;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class LoanController : Controller
    {
        private readonly IEzbobWorkplaceContext _context;
        private readonly PaymentRolloverRepository _rolloverRepository;

        public LoanController(IEzbobWorkplaceContext context, PaymentRolloverRepository rolloverRepository)
        {
            _context = context;
            _rolloverRepository = rolloverRepository;
        }

        [Transactional]
        [ValidateJsonAntiForgeryToken]
        [HttpGet]
        public JsonNetResult Details(int id)
        {
            var customer = _context.Customer;

            var loan = customer.Loans.SingleOrDefault(l => l.Id == id);
            
            if (loan == null)
            {
                return this.JsonNet(new { error = "loan does not exists" });
            }

            var loansDetailsBuilder= new LoansDetailsBuilder();
            var details = loansDetailsBuilder.Build(loan, _rolloverRepository.GetByLoanId(loan.Id));

            return this.JsonNet(details);
        }

        public JsonNetResult Get(int id)
        {
            var customer = _context.Customer;
            var loan = customer.Loans.SingleOrDefault(l => l.Id == id);

            IPayEarlyCalculator calculator = new LoanRepaymentScheduleCalculator(loan, DateTime.UtcNow);

            var loanModel = LoanModel.FromLoan(loan, calculator);

            return this.JsonNet(loanModel);
            
        }
    }
}
