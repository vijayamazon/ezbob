namespace EzBob.Web.Areas.Customer.Controllers {
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.Model.Database;
    using CommonLib;
    using Code;
    using Infrastructure;
    using Infrastructure.Attributes;
    using Infrastructure.csrf;
    using PaymentServices.Calculators;
    using ServiceClientProxy;
    using StructureMap;
    using Web.Models;

    public class ScheduleController : Controller {
        private readonly APRCalculator aprCalc;
        private readonly IEzbobWorkplaceContext context;
        private readonly LoanBuilder loanBuilder;
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ScheduleController));
        public ServiceClient _oServiceClient { get; set; }


        public ScheduleController(IEzbobWorkplaceContext context, LoanBuilder loanBuilder) {
            this.context = context;
            this.loanBuilder = loanBuilder;
            this.aprCalc = new APRCalculator();
            _oServiceClient = new ServiceClient();
        }


        // constructor

        [Ajax]
        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        [Transactional]
        public JsonResult Calculate(int amount, int loanType, int repaymentPeriod) {
            // el: slider of offer display (customer app)
            LoanOffer loanOffer = CalculateLoan(amount, loanType, repaymentPeriod);

            var productSubTypeID = this.context.Customer.LastCashRequest.ProductSubTypeID;
            var originId = this.context.Customer.CustomerOrigin.CustomerOriginID;
            var isRegulated = this.context.Customer.PersonalInfo.TypeOfBusiness.IsRegulated();

            loanOffer.Templates = _oServiceClient.Instance.GetLegalDocs(this.context.UserId, this.context.UserId, originId, isRegulated, productSubTypeID ?? 0).LoanAgreementTemplates.ToList();

            if (loanOffer == null)
                return Json(new { error = "Invalid customer state" }, JsonRequestBehavior.AllowGet);

            return Json(loanOffer, JsonRequestBehavior.AllowGet);
        } // Calculate

        private LoanOffer CalculateLoan(int amount, int loanType, int repaymentPeriod) {
            if (!this.context.Customer.CreditSum.HasValue || !this.context.Customer.Status.HasValue || this.context.Customer.Status.Value != Status.Approved)
                return null;

            var creditSum = this.context.Customer.CreditSum.Value;

            this.context.Customer.ValidateOfferDate();

            if (amount < 0)
                amount = (int)Math.Floor(creditSum);

            if (amount > creditSum) {
                Log.WarnFormat("Attempt to calculate schedule for amount({0}) bigger than credit sum value({1})", amount, creditSum);
                amount = (int)Math.Floor(creditSum);
            } // if

            var cr = this.context.Customer.LastCashRequest;

            if (this.context.Customer.IsLoanTypeSelectionAllowed == 1) {
                var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;

                if (oDBHelper != null) {

                    cr.LoanType = oDBHelper.LoanTypeRepository.Get(loanType);
                } // if
            } // if

            if (cr.IsCustomerRepaymentPeriodSelectionAllowed) {
                cr.RepaymentPeriod = repaymentPeriod;
            }

            var loan = this.loanBuilder.CreateLoan(cr, amount, DateTime.UtcNow);

            var schedule = loan.Schedule;
            var apr = this.aprCalc.Calculate(amount, schedule, loan.SetupFee, loan.Date);

            var b = new AgreementsModelBuilder();
            var agreement = b.Build(this.context.Customer, amount, loan);

            //TODO calculate offer
            Log.DebugFormat("calculate offer for customer {0}", this.context.Customer.Id);

            return LoanOffer.InitFromLoan(loan, apr, agreement, cr);
        } // CalculateLoan
    } // class ScheduleController
} // namespace
