﻿using System;
using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
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
        private readonly IPacNetBalanceRepository _funds;
        private readonly IDiscountPlanRepository _discounts;
        private readonly CashRequestBuilder _crBuilder;
        private readonly RepaymentCalculator _repaymentCalculator = new RepaymentCalculator();


        private static readonly ILog Log = LogManager.GetLogger(typeof (ApplicationInfoController));

        public ApplicationInfoController(ICustomerRepository customerRepository, ICashRequestsRepository cashRequestsRepository,
                                         IApplicationRepository applications, IEzBobConfiguration config,
                                         IZohoFacade crm, ILoanTypeRepository loanTypes, LoanLimit limit,
                                            IPacNetBalanceRepository funds, IDiscountPlanRepository discounts, CashRequestBuilder crBuilder)
        {
            _customerRepository = customerRepository;
            _cashRequestsRepository = cashRequestsRepository;
            _applications = applications;
            _config = config;
            _crm = crm;
            _loanTypes = loanTypes;
            _limit = limit;
            _funds = funds;
            _discounts = discounts;
            _crBuilder = crBuilder;
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
            InitApplicationInfo(m, customer, cr);
            return this.JsonNet(m);
        }

        private void InitApplicationInfo(ApplicationInfoModel model, EZBob.DatabaseLib.Model.Database.Customer customer, CashRequest cr)
        {
            if (customer == null) return;

            model.Id = customer.Id;

            if (cr != null)
            {
                model.InterestRate = cr.InterestRate;
                model.CashRequestId = cr.Id;
                model.UseSetupFee = cr.UseSetupFee;
                model.AllowSendingEmail = !cr.EmailSendingBanned;

                var loanType = cr.LoanType ?? _loanTypes.GetAll().First();

                model.LoanType = loanType.Name;
                model.LoanTypeId = loanType.Id;
                
                cr.OfferStart = cr.OfferStart ?? customer.OfferStart;
                cr.OfferValidUntil = cr.OfferValidUntil ?? customer.OfferValidUntil;

                model.RepaymentPerion = _repaymentCalculator.ReCalculateRepaymentPeriod(cr);
            }

            model.CustomerId = customer.Id;
            model.IsTest = customer.IsTest;
            model.SystemDecision = customer.Status.ToString();

            if (cr.SystemCalculatedSum != null && cr.SystemCalculatedSum.Value != 0)
            {
                model.SystemCalculatedAmount = Convert.ToDecimal(cr.SystemCalculatedSum.Value);
            }


            model.OfferedCreditLine = Convert.ToDecimal(cr.ManagerApprovedSum ?? cr.SystemCalculatedSum);
            model.BorrowedAmount = customer.Loans.Where(x => x.CashRequest != null && x.CashRequest.Id == cr.Id).Sum(x => x.LoanAmount);
            model.AvaliableAmount = customer.CreditSum ?? 0M;
            model.OfferExpired = cr.OfferValidUntil <= DateTime.UtcNow;

            model.StartingFromDate = FormattingUtils.FormatDateToString(cr.OfferStart);
            model.OfferValidateUntil = FormattingUtils.FormatDateToString(cr.OfferValidUntil);

            model.FundsAvaliable = FormattingUtils.FormatPounds(_funds.GetFunds());
            //Status = "Active";
            model.Details = customer.Details;
            var isWaitingOrEscalated = customer.CreditResult == CreditResultStatus.WaitingForDecision ||
                                       customer.CreditResult == CreditResultStatus.Escalated;

            var isEnabled = customer.CollectionStatus.CurrentStatus == CollectionStatusType.Enabled;
            model.Editable = isWaitingOrEscalated && cr != null && isEnabled;

            model.IsModified = !string.IsNullOrEmpty(cr.LoanTemplate);

            model.LoanTypes = _loanTypes.GetAll().Select(t => LoanTypesModel.Create(t)).ToArray();

            model.DiscountPlans = _discounts.GetAll().Select(d => DiscountPlanModel.Create(d)).ToArray();
            var discountPlan = (cr.DiscountPlan ?? _discounts.GetDefault());
            model.DiscountPlan = discountPlan.Name;
            model.DiscountPlanId = discountPlan.Id;

            model.Reason = cr.UnderwriterComment;

            model.IsLoanTypeSelectionAllowed = cr.IsLoanTypeSelectionAllowed;
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
        public JsonNetResult RunNewCreditLine(int Id)
        {
            var anyApps = _applications.StratagyIsRunning(Id, _config.ScoringResultStrategyName);
            if (anyApps)
                return this.JsonNet(new { Message = "The evaluation strategy is already running. Please wait..." });

            var customer = _customerRepository.Get(Id);

            var cashRequest = _crBuilder.CreateCashRequest(customer);
            cashRequest.LoanType = _loanTypes.GetDefault();

            _crBuilder.ForceEvaluate(customer, false);

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
        public JsonNetResult ChangeCreditLine(long id, int loanType, double amount, decimal interestRate, int repaymentPeriod, string offerStart, string offerValidUntil, bool useSetupFee, bool allowSendingEmail, int isLoanTypeSelectionAllowed)
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

            _crm.UpdateCashRequest(cr);
       
            return this.JsonNet(true);
        }
    }
}
