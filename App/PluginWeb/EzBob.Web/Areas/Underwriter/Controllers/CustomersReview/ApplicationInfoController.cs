using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.csrf;
using Scorto.Web;
using ZohoCRM;
using log4net;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class ApplicationInfoController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICashRequestsRepository _cashRequestsRepository;
        private readonly IApplicationRepository _applications;
        private readonly IEzBobConfiguration _config;
        private readonly IZohoFacade _crm;
        private readonly ILoanTypeRepository _loanTypes;
        private readonly LoanLimit _limit;
        private readonly IDiscountPlanRepository _discounts;
        private readonly CashRequestBuilder _crBuilder;
        private readonly ApplicationInfoModelBuilder _infoModelBuilder;

        private static readonly ILog Log = LogManager.GetLogger(typeof (ApplicationInfoController));

        public ApplicationInfoController(ICustomerRepository customerRepository, ICashRequestsRepository cashRequestsRepository,
                                         IApplicationRepository applications, IEzBobConfiguration config,
                                         IZohoFacade crm, ILoanTypeRepository loanTypes, LoanLimit limit,
                                            IDiscountPlanRepository discounts, CashRequestBuilder crBuilder,ApplicationInfoModelBuilder infoModelBuilder)
        {
            _customerRepository = customerRepository;
            _cashRequestsRepository = cashRequestsRepository;
            _applications = applications;
            _config = config;
            _crm = crm;
            _loanTypes = loanTypes;
            _limit = limit;
            _discounts = discounts;
            _crBuilder = crBuilder;
            _infoModelBuilder = infoModelBuilder;
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpGet]
        [Transactional]
        public JsonNetResult Index(int id)
        {
            var customer = _customerRepository.Get(id);
            var m = new ApplicationInfoModel();
            var cr = customer.LastCashRequest;
            _infoModelBuilder.InitApplicationInfo(m, customer, cr);
            return this.JsonNet(m);
        }



        [HttpPost]
        [Transactional]
        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [Permission(Name = "CreditLineFields")]
        public JsonNetResult ChangeCashRequestOpenCreditLine( long id, double amount)
        {
            _limit.Check(amount);
            var cr = _cashRequestsRepository.Get(id);
            cr.ManagerApprovedSum = amount;
            _crm.UpdateCashRequest(cr);
            cr.LoanTemplate = null;
            _cashRequestsRepository.SaveOrUpdate(cr);

            Log.DebugFormat("CashRequest({0}).ManagerApprovedSum = {1}", id, cr.ManagerApprovedSum);

            return this.JsonNet(true);
        }

        [HttpPost]
        [Ajax]
        [Transactional]
        [Permission(Name = "CreditLineFields")]
        public void LoanType(long id, int loanType)
        {
            var cr = _cashRequestsRepository.Get(id);
            var loanT = _loanTypes.Get(loanType);
            cr.LoanType = loanT;
            cr.RepaymentPeriod = loanT.RepaymentPeriod;
            cr.LoanTemplate = null;
            Log.DebugFormat("CashRequest({0}).LoanType = {1}", id, cr.LoanType.Name);
        }

        [HttpPost]
        [Ajax]
        [Transactional]
        public JsonNetResult DiscountPlan(long id, int discountPlanId)
        {
            var cr = _cashRequestsRepository.Get(id);
            var discount = _discounts.Get(discountPlanId);
            cr.DiscountPlan = discount;
            //cr.LoanTemplate = null;
            //Log.DebugFormat("CashRequest({0}).Discount = {1}", id, cr.DiscountPlan.Name);
            return this.JsonNet(new {});
        }

        [HttpPost]
        [Transactional]
        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [Permission(Name = "CreditLineFields")]
        public JsonNetResult ChangeCashRequestInterestRate(long id, decimal interestRate)
        {
            var cr = _cashRequestsRepository.Get(id);
            cr.InterestRate = interestRate/100;
            cr.LoanTemplate = null;

            Log.DebugFormat("CashRequest({0}).InterestRate = {1}", id, cr.InterestRate);

            return this.JsonNet(true);
        }

        [HttpPost]
        [Transactional]
        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [Permission(Name = "CreditLineFields")]
        public JsonNetResult ChangeCashRequestRepaymentPeriod(long id, int period)
        {
            var cr = _cashRequestsRepository.Get(id);
            cr.RepaymentPeriod = period;
            cr.LoanTemplate = null;

            Log.DebugFormat("CashRequest({0}).RepaymentPeriod = {1}", id, period);

            return this.JsonNet(true);
        }

        [HttpPost]
        [Transactional]
        [ValidateJsonAntiForgeryToken]
        [Ajax]
        public void SaveDetails(int id, string details)
        {
            var cust = _customerRepository.Get(id);
            if (cust == null) return;

            cust.Details = details;
        }

        [HttpPost]
        [Transactional]
        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [Permission(Name = "CreditLineFields")]
        public void ChangeSetupFee(long id, bool enbaled)
        {
            var cr = _cashRequestsRepository.Get(id);
            cr.UseSetupFee = enbaled;
            cr.LoanTemplate = null;
            Log.DebugFormat("CashRequest({0}).UseSetupFee = {1}", id, enbaled);
        }

        [HttpPost]
        [Transactional]
        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [Permission(Name = "TestUser")]
        public void ChangeTestStatus(int id, bool enbaled)
        {
            var cust = _customerRepository.Get(id);
            cust.IsTest = enbaled;
            _crm.UpdateCustomer(cust);
            Log.DebugFormat("Customer({0}).IsTest = {1}", id, enbaled);
        }

        [HttpPost]
        [Transactional]
        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [Permission(Name = "CreditLineFields")]
        public void AllowSendingEmails(long id, bool enbaled)
        {
            var cr = _cashRequestsRepository.Get(id);
            cr.EmailSendingBanned = !enbaled;
            cr.LoanTemplate = null;
            Log.DebugFormat("CashRequest({0}).EmailSendingBanned = {1}", id, cr.EmailSendingBanned);
        }

        [HttpPost]
        [Transactional]
        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [Permission(Name = "CreditLineFields")]
        public void IsLoanTypeSelectionAllowed(long id, int loanTypeSelection)
        {
            var cr = _cashRequestsRepository.Get(id);
            cr.IsLoanTypeSelectionAllowed = loanTypeSelection;
            Log.DebugFormat("CashRequest({0}).IsLoanTypeSelectionAllowed = {1}", id, cr.IsLoanTypeSelectionAllowed);
        }

        [HttpPost]
        [Transactional]
        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [Permission(Name = "CreditLineFields")]
        public void ChangeOferValid(int id, string date)
        {
            var cust = _customerRepository.Get(id);
            if (cust == null) return;

            Log.DebugFormat("CashRequest({0}).OfferValidUntil = {1}", id, date);

            var dt = FormattingUtils.ParseDateWithCurrentTime(date);
            var cr = cust.LastCashRequest;
            cr.OfferValidUntil = dt;
            cr.LoanTemplate = null;
            _crm.UpdateCashRequest(cr);

        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [Transactional]
        [Ajax]
        [Permission(Name = "CreditLineFields")]
        public void ChangeStartingDate(int id, string date)
        {
            var cust = _customerRepository.Get(id);
            if (cust == null) return;

            Log.DebugFormat("CashRequest({0}).OfferStart = {1}", id, date);
            Log.DebugFormat("CashRequest({0}).OfferValidUntil = {1}", id, date);

            var dt = FormattingUtils.ParseDateWithCurrentTime(date);

            var cr = cust.LastCashRequest;
            cr.OfferStart = dt;
            cr.OfferValidUntil = dt.AddDays(1);
            cr.LoanTemplate = null;
            _crm.UpdateCashRequest(cr);
        }


        [HttpPost]
        [Transactional]
        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [Permission(Name = "NewCreditLineButton")]
        public JsonNetResult RunNewCreditLine(int Id, int newCreditLineOption)
        {
            var anyApps = _applications.StratagyIsRunning(Id, _config.ScoringResultStrategyName);
            if (anyApps)
                return this.JsonNet(new { Message = "The evaluation strategy is already running. Please wait..." });

            var customer = _customerRepository.Get(Id);

            var cashRequest = _crBuilder.CreateCashRequest(customer);
            cashRequest.LoanType = _loanTypes.GetDefault();

            _crBuilder.ForceEvaluate(customer, (NewCreditLineOption) newCreditLineOption, false);

            customer.OfferStart = cashRequest.OfferStart;
            customer.OfferValidUntil = cashRequest.OfferValidUntil;
            customer.CreditResult = null;

            _crm.UpdateCashRequest(cashRequest);

            _crm.CreateOffer(customer, cashRequest);

            return this.JsonNet(new { Message = "The evaluation has been started. Please refresh this application after a while..." });
        }

        [HttpPost]
        [Transactional]
        [Ajax]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult ChangeCreditLine(long id, int loanType, double amount, decimal interestRate, int repaymentPeriod, string offerStart, string offerValidUntil, bool useSetupFee, bool allowSendingEmail, int isLoanTypeSelectionAllowed, int discountPlan)
        {
            var cr = _cashRequestsRepository.Get(id);
            var loanT = _loanTypes.Get(loanType);
            cr.LoanType = loanT;
            cr.ManagerApprovedSum = amount;
            cr.InterestRate = interestRate;
            cr.RepaymentPeriod = repaymentPeriod;
            cr.OfferStart = FormattingUtils.ParseDateWithCurrentTime(offerStart);
            cr.OfferValidUntil = FormattingUtils.ParseDateWithCurrentTime(offerValidUntil);
            cr.UseSetupFee = useSetupFee;
            cr.EmailSendingBanned = !allowSendingEmail;
            cr.LoanTemplate = null;
            cr.IsLoanTypeSelectionAllowed = isLoanTypeSelectionAllowed;
            cr.DiscountPlan = _discounts.Get(discountPlan);

            _crm.UpdateCashRequest(cr);
       
            return this.JsonNet(true);
        }
    }
}
