using System;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EzBob.CommonLib;
using EzBob.Models;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Code;
using EzBob.Web.Code.Agreements;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.csrf;
using PaymentServices.Calculators;
using Scorto.Web;
using StructureMap;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IEzbobWorkplaceContext _context;
        private readonly APRCalculator _aprCalc;
        private readonly EZBob.DatabaseLib.Model.Database.Customer _customer;
        private readonly CustomerModelBuilder _customerModelBuilder;
        private readonly LoanBuilder _loanBuilder;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ScheduleController));

        public ScheduleController(IEzbobWorkplaceContext context, CustomerModelBuilder customerModelBuilder, LoanBuilder loanBuilder)
        {
            _context = context;
            _customerModelBuilder = customerModelBuilder;
            _loanBuilder = loanBuilder;
            _customer = _context.Customer;
            _aprCalc = new APRCalculator();
        }

        [Ajax]
        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        [Transactional]
        public JsonNetResult Calculate(int amount, int loanType, int repaymentPeriod)
        {
            if (!_customer.CreditSum.HasValue || !_customer.Status.HasValue || _customer.Status.Value != Status.Approved)
            {
                return this.JsonNet(new{error="Invalid customer state"});
            }

            _customer.ValidateOfferDate();

            if (amount < 0)
            {
                amount = (int)Math.Floor(_customer.CreditSum.Value);
            }

            if(amount > _customer.CreditSum.Value)
            {
                Log.WarnFormat("Attempt to calculate schedule for ammount({0}) bigger than credit sum value({1})", amount, _customer.CreditSum);
                amount = (int)Math.Floor(_customer.CreditSum.Value);
            }

            var cr = _customer.LastCashRequest;

			if (_customer.IsLoanTypeSelectionAllowed == 1) {
				var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
				cr.RepaymentPeriod = repaymentPeriod;
				cr.LoanType = oDBHelper.LoanTypeRepository.Get(loanType);
			} // if

	        var loan = _loanBuilder.CreateLoan(cr, amount, DateTime.UtcNow);

            var schedule = loan.Schedule;
            var apr = _aprCalc.Calculate(amount, schedule, loan.SetupFee);

            var b = new AgreementsModelBuilder(_customerModelBuilder);
            var agreement = b.Build(_customer, amount, loan);
            var loanOffer = LoanOffer.InitFromLoan(loan, apr, agreement);

            return this.JsonNet(loanOffer);
        }
    }
}