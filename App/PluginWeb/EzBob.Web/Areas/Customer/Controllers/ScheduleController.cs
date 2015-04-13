namespace EzBob.Web.Areas.Customer.Controllers {
    using System;
    using System.Web.Mvc;
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.Model.Database;
    using CommonLib;
    using Code;
    using Infrastructure;
    using Infrastructure.Attributes;
    using Infrastructure.csrf;
    using PaymentServices.Calculators;
    using StructureMap;
    using Web.Models;

    public class ScheduleController : Controller {
        public static readonly int[] LoanPeriods = { 6, 10 };

        private readonly IEzbobWorkplaceContext _context;
        private readonly APRCalculator _aprCalc;
        private readonly Customer _customer;
        private readonly LoanBuilder _loanBuilder;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ScheduleController));

        public ScheduleController(IEzbobWorkplaceContext context, LoanBuilder loanBuilder) {
            _context = context;
            _loanBuilder = loanBuilder;
            _customer = _context.Customer;
            _aprCalc = new APRCalculator();
        } // constructor

        [Ajax]
        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        [Transactional]
        public JsonResult Calculate(int amount, int loanType, int repaymentPeriod) {
            LoanOffer loanOffer = CalculateLoan(amount, loanType, repaymentPeriod);

            if (loanOffer == null)
                return Json(new { error = "Invalid customer state" }, JsonRequestBehavior.AllowGet);

            return Json(loanOffer, JsonRequestBehavior.AllowGet);
        } // Calculate

        private LoanOffer CalculateLoan(int amount, int loanType, int repaymentPeriod) {
            if (!_customer.CreditSum.HasValue || !_customer.Status.HasValue || _customer.Status.Value != Status.Approved)
                return null;

            _customer.ValidateOfferDate();

            if (amount < 0)
                amount = (int)Math.Floor(_customer.CreditSum.Value);

            if (amount > _customer.CreditSum.Value) {
                Log.WarnFormat("Attempt to calculate schedule for ammount({0}) bigger than credit sum value({1})", amount, _customer.CreditSum);
                amount = (int)Math.Floor(_customer.CreditSum.Value);
            } // if

            var cr = _customer.LastCashRequest;

            if (_customer.IsLoanTypeSelectionAllowed == 1) {
                var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;

                if (oDBHelper != null) {
                    cr.RepaymentPeriod = repaymentPeriod;
                    cr.LoanType = oDBHelper.LoanTypeRepository.Get(loanType);
                } // if
            } // if

            var loan = _loanBuilder.CreateLoan(cr, amount, DateTime.UtcNow);

            var schedule = loan.Schedule;
            var apr = _aprCalc.Calculate(amount, schedule, loan.SetupFee, loan.Date);

            var b = new AgreementsModelBuilder();
            var agreement = b.Build(_customer, amount, loan);

            //TODO calculate offer
            Log.DebugFormat("calculate offer for customer {0}", _customer.Id);

            return LoanOffer.InitFromLoan(loan, apr, agreement, cr);
        } // CalculateLoan
    } // class ScheduleController
} // namespace
