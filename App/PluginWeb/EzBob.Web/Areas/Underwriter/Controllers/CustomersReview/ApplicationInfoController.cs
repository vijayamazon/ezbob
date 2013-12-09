﻿using EZBob.DatabaseLib.Model.Database.Loans;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Loans;
	using ApplicationCreator;
	using Models;
	using Code;
	using Infrastructure;
	using Infrastructure.csrf;
	using Scorto.Web;
	using log4net;

	public class ApplicationInfoController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICashRequestsRepository _cashRequestsRepository;
        private readonly IApplicationRepository _applications;
        private readonly IEzBobConfiguration _config;
        private readonly ILoanTypeRepository _loanTypes;
        private readonly LoanLimit _limit;
        private readonly IDiscountPlanRepository _discounts;
        private readonly CashRequestBuilder _crBuilder;
		private readonly ApplicationInfoModelBuilder _infoModelBuilder;
		private readonly IPacNetManualBalanceRepository _pacNetManualBalanceRepository;
		private readonly ICustomerStatusesRepository customerStatusesRepository;
		private readonly IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository;
		private readonly IConfigurationVariablesRepository configurationVariablesRepository;
		private readonly ICustomerStatusHistoryRepository customerStatusHistoryRepository;
		private readonly ILoanSourceRepository _loanSources;

        private static readonly ILog Log = LogManager.GetLogger(typeof (ApplicationInfoController));

		public ApplicationInfoController(
			ICustomerRepository customerRepository,
			ICustomerStatusesRepository customerStatusesRepository,
			ICashRequestsRepository cashRequestsRepository,
			IApplicationRepository applications,
			IEzBobConfiguration config,
			ILoanTypeRepository loanTypes,
			LoanLimit limit,
			IDiscountPlanRepository discounts,
			CashRequestBuilder crBuilder,
			ApplicationInfoModelBuilder infoModelBuilder,
			IPacNetManualBalanceRepository pacNetManualBalanceRepository,
			IApprovalsWithoutAMLRepository approvalsWithoutAMLRepository,
			IConfigurationVariablesRepository configurationVariablesRepository,
			ICustomerStatusHistoryRepository customerStatusHistoryRepository,
			ILoanSourceRepository loanSources
		)
		{
			_customerRepository = customerRepository;
			_cashRequestsRepository = cashRequestsRepository;
			_applications = applications;
			_config = config;
			_loanTypes = loanTypes;
			_limit = limit;
			_discounts = discounts;
			_crBuilder = crBuilder;
			_infoModelBuilder = infoModelBuilder;
			_pacNetManualBalanceRepository = pacNetManualBalanceRepository;
			this.customerStatusesRepository = customerStatusesRepository;
			this.approvalsWithoutAMLRepository = approvalsWithoutAMLRepository;
			this.configurationVariablesRepository = configurationVariablesRepository;
			this.customerStatusHistoryRepository = customerStatusHistoryRepository;
			_loanSources = loanSources;
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

		[Ajax]
        [OutputCache(VaryByParam = "status", Duration = int.MaxValue)]
		public JsonNetResult GetIsStatusEnabled(int status)
		{
			bool res = customerStatusesRepository.GetIsEnabled(status);
			return this.JsonNet(res);
		}

		[Ajax]
        [OutputCache(VaryByParam = "status", Duration = int.MaxValue)]
		public JsonNetResult GetIsStatusWarning(int status)
		{
			bool res = customerStatusesRepository.GetIsWarning(status);
			return this.JsonNet(res);
		}

		[Ajax]
		[Transactional]
		public void LogStatusChange(int newStatus, int prevStatus, int customerId)
		{
			var newEntry = new CustomerStatusHistory();
			newEntry.Username = User.Identity.Name;
			newEntry.Timestamp = DateTime.UtcNow;
			newEntry.CustomerId = customerId;
			newEntry.PreviousStatus = prevStatus;
			newEntry.NewStatus = newStatus;
			customerStatusHistoryRepository.SaveOrUpdate(newEntry);
		}

		[HttpPost]
		[Transactional]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "CreditLineFields")]
		public JsonNetResult ChangeCashRequestOpenCreditLine(long id, double amount)
		{
			_limit.Check(amount);
			var cr = _cashRequestsRepository.Get(id);
			int step = _config.GetCashSliderStep;
			cr.ManagerApprovedSum = Math.Round(amount / step, MidpointRounding.AwayFromZero) * step;
			cr.LoanTemplate = null;
			_cashRequestsRepository.SaveOrUpdate(cr);

			Log.DebugFormat("CashRequest({0}).ManagerApprovedSum = {1}", id, cr.ManagerApprovedSum);

			return this.JsonNet(true);
		}

		[HttpPost]
		[Transactional]
		[Ajax]
		public void SaveApproveWithoutAML(int customerId, bool doNotShowAgain)
		{
			Log.DebugFormat("Saving approve without AML. Customer:{0} doNotShowAgain = {1}", customerId, doNotShowAgain);
			
			var entry = new ApprovalsWithoutAML
				{
					CustomerId = customerId,
					DoNotShowAgain = doNotShowAgain,
					Timestamp = DateTime.UtcNow,
					Username = User.Identity.Name
				};

			approvalsWithoutAMLRepository.SaveOrUpdate(entry);
		}

		[HttpPost]
		[Transactional]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "PacnetManualButton")]
		public JsonNetResult SavePacnetManual(int amount, int limit)
		{
			var newEntry = new PacNetManualBalance
			{
				Date = DateTime.UtcNow,
				Enabled = true,
				Amount = amount,
				Username = User.Identity.Name
			};

			_pacNetManualBalanceRepository.SaveOrUpdate(newEntry);
			return this.JsonNet(true);
		}

		[HttpPost]
		[Transactional]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		[Permission(Name = "PacnetManualButton")]
		public JsonNetResult DisableTodaysPacnetManual(bool isSure)
		{
			if (isSure)
			{
				_pacNetManualBalanceRepository.DisableTodays();
			}
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
		[Ajax]
		[Transactional]
		public JsonNetResult LoanSource(long id, int LoanSourceID) {
			var cr = _cashRequestsRepository.Get(id);
			cr.LoanSource = _loanSources.Get(LoanSourceID);

			if (cr.LoanSource == null)
				cr.IsCustomerRepaymentPeriodSelectionAllowed = true;
			else {
				cr.IsCustomerRepaymentPeriodSelectionAllowed = cr.LoanSource.IsCustomerRepaymentPeriodSelectionAllowed;

				if (cr.LoanSource.DefaultRepaymentPeriod.HasValue)
					cr.RepaymentPeriod = cr.LoanSource.DefaultRepaymentPeriod.Value;
			} // if

			return this.JsonNet(new { });
		} // LoanSource

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
            Log.DebugFormat("Customer({0}).IsTest = {1}", id, enbaled);
        }

		[HttpPost]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		public JsonNetResult ToggleCciMark(int id) {
			Customer oCustomer = _customerRepository.Get(id);

			if (oCustomer == null) {
				Log.DebugFormat("Customer({0}) not found", id);
				return this.JsonNet(new { error = "Customer not found.", id = id });
			} // if

			oCustomer.CciMark = !oCustomer.CciMark;
			Log.DebugFormat("Customer({0}).CciMark set to {1}", id, oCustomer.CciMark);

			return this.JsonNet(new { error = (string)null, id = id, mark = oCustomer.CciMark });
		} // ToggleCciMark

        [HttpPost]
        [Transactional]
        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [Permission(Name = "CreditLineFields")]
        public void AvoidAutomaticDecision(int id, bool enbaled)
        {
            var cust = _customerRepository.Get(id);
            cust.IsAvoid = enbaled;
            Log.DebugFormat("Customer({0}).IsAvoided = {1}", id, enbaled);
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
	        cust.OfferValidUntil = dt;
            var cr = cust.LastCashRequest;
            cr.LoanTemplate = null;
	        cr.OfferValidUntil = dt;
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

	        int offerValidForHours = (int)configurationVariablesRepository.GetByNameAsDecimal("OfferValidForHours");

            var cr = cust.LastCashRequest;
	        cust.OfferStart = dt;
			var offerValidUntil = dt.AddHours(offerValidForHours);
	        cust.OfferValidUntil = offerValidUntil;
            cr.LoanTemplate = null;
	        cr.OfferStart = dt;
	        cr.OfferValidUntil = offerValidUntil;
        }


        [HttpPost]
        [Transactional]
        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [Permission(Name = "NewCreditLineButton")]
        public JsonNetResult RunNewCreditLine(int Id, int newCreditLineOption)
        {
	        if (!_config.SkipServiceOnNewCreditLine) {
		        var anyApps = _applications.StratagyIsRunning(Id, _config.ScoringResultStrategyName);
		        if (anyApps)
			        return this.JsonNet(new {Message = "The evaluation strategy is already running. Please wait..."});
	        }

	        var customer = _customerRepository.Get(Id);

            var cashRequest = _crBuilder.CreateCashRequest(customer);
            cashRequest.LoanType = _loanTypes.GetDefault();

            _crBuilder.ForceEvaluate(customer, (NewCreditLineOption) newCreditLineOption, false);

            customer.CreditResult = null;
			customer.OfferStart = cashRequest.OfferStart;
			customer.OfferValidUntil = cashRequest.OfferValidUntil;
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

			Customer c = cr.Customer;
			c.OfferStart = cr.OfferStart;
			c.OfferValidUntil = cr.OfferValidUntil;
       
            return this.JsonNet(true);
        }
    }
}
