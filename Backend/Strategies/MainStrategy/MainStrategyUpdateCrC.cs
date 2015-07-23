namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using ConfigManager;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database.Loans;

	// This class contains lots of empty property setters.
	// They must exist because these properties are accessed via reflection and found by having a public set.

	[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
	internal class MainStrategyUpdateCrC : AStoredProcedure {
		public MainStrategyUpdateCrC(
			int customerID,
			long cashRequestID,
			AutoDecisionResponse autoDecisionResponse,
			LastOfferData lastOffer,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			this.now = DateTime.UtcNow;

			this.customerID = customerID;
			CashRequestID = cashRequestID;
			this.autoDecisionResponse = autoDecisionResponse;
			this.lastOffer = lastOffer;

			this.repaymentPeriodToUse = this.autoDecisionResponse.RepaymentPeriod;
			this.isCustomerRepaymentPeriodSelectionAllowedToUse =
				this.autoDecisionResponse.IsCustomerRepaymentPeriodSelectionAllowed;

			if (this.autoDecisionResponse.IsAutoApproval) {
				this.interestRateToUse = this.autoDecisionResponse.InterestRate;
				this.setupFeePercentToUse = this.autoDecisionResponse.SetupFee;
			} else { //TODO check this code!!!
				this.interestRateToUse = this.lastOffer.LoanOfferInterestRate;
				this.setupFeePercentToUse = this.lastOffer.ManualSetupFeePercent;
			} // if

			InitLoanSource();
			InitDiscountPlan();
			InitLoanType();
		} // constructor

		public override bool HasValidParameters() {
			return
				(CustomerID > 0) &&
				(CashRequestID > 0)
			;
		} // HasValidParameters

		// Stored procedure arguments - begin

		public int CustomerID {
			get { return this.customerID; }
			set { }
		} // CustomerID

		public long CashRequestID { get; set; } // CashRequestID

		public bool OverrideApprovedRejected { get; set; }

		public string CustomerStatus {
			get {
				return this.autoDecisionResponse.UserStatus.HasValue
					? this.autoDecisionResponse.UserStatus.Value.ToString()
					: null;
			} // get
			set { }
		} // CustomerStatus

		public DateTime Now {
			get { return this.now; }
			set { }
		} // Now

		public DateTime OfferValidUntil {
			get { return Now.AddHours(CurrentValues.Instance.OfferValidForHours); }
			set { }
		} // OfferValidUntil

		public string SystemDecision {
			get {
				return this.autoDecisionResponse.SystemDecision.HasValue
					? this.autoDecisionResponse.SystemDecision.Value.ToString()
					: null;
			} // get
			set { }
		} // SystemDecision

		public string MedalClassification { get; set; }
		public decimal OfferedCreditLine { get; set; }

		public string CreditResult {
			get {
				return this.autoDecisionResponse.CreditResult.HasValue
					? this.autoDecisionResponse.CreditResult.Value.ToString()
					: null;
			} // get
			set { }
		} // CreditResult

		public decimal SystemCalculatedSum { get; set; }

		public bool DecidedToReject {
			get { return this.autoDecisionResponse.DecidedToReject; }
			set { }
		} // DecidedToReject

		public bool DecidedToApprove {
			get { return this.autoDecisionResponse.DecidedToApprove; }
			set { }
		} // DecidedToApprove

		public bool IsLoanTypeSelectionAllowed {
			// Currently (July 2015) it is always false. It was true/false in the past and may be such in the future.
			get { return false; }
			set { }
		} // IsLoanTypeSelectionAllowed

		public string Reason {
			get { return this.autoDecisionResponse.DecisionName; }
			set { }
		} // Reason

		public int? AutoDecisionID {
			get { return this.autoDecisionResponse.DecisionCode; }
			set { }
		} // AutoDecisionID

		public string AutoDecisionName {
			get {
				return this.autoDecisionResponse.Decision == null
					? null
					: this.autoDecisionResponse.Decision.Value.ToString();
			} // get
			set { }
		} // AutoDecisionName

		public decimal TotalScoreNormalized { get; set; }
		public int ExperianConsumerScore { get; set; }
		public int AnnualTurnover { get; set; }

		public int LoanTypeID {
			get { return this.loanTypeIDToUse; }
			set { }
		} // LoanTypeID

		public int LoanSourceID {
			get { return this.loanSourceToUse.ID; }
			set { }
		} // LoanSourceID

		public decimal InterestRate {
			get { return this.interestRateToUse; }
			set { }
		} // InterestRate

		public int RepaymentPeriod {
			get { return this.repaymentPeriodToUse; }
			set { }
		} // RepaymentPeriod

		public decimal ManualSetupFeePercent {
			get { return this.setupFeePercentToUse; }
			set { }
		} // ManualSetupFeePercent

		public decimal Apr {
			get { return this.lastOffer.LoanOfferApr; }
			set { }
		} // Apr

		public bool IsCustomerRepaymentPeriodSelectionAllowed {
			get { return this.isCustomerRepaymentPeriodSelectionAllowedToUse; }
			set { }
		} // IsCustomerRepaymentPeriodSelectionAllowed

		public bool EmailSendingBanned {
			get { return this.autoDecisionResponse.LoanOfferEmailSendingBannedNew; }
			set { }
		} // EmailSendingBanned

		public int DiscountPlanID {
			get { return this.discountPlanIDToUse; }
			set { }
		} // DiscountPlanID

		public bool HasApprovalChance {
			get { return this.autoDecisionResponse.HasApprovalChance; }
			set { }
		} // HasApprovalChance

		// Stored procedure arguments - end

		private void InitLoanSource() {
			SafeReader lssr = DB.GetFirst(
				"GetLoanSource",
				CommandSpecies.StoredProcedure,
				new QueryParameter( // Get specific for re-approval or default otherwise
					"@LoanSourceID",
					this.autoDecisionResponse.IsAutoReApproval ? this.autoDecisionResponse.LoanSourceID : (int?)null
				)
			);

			this.loanSourceToUse = lssr.Fill<LoanSource>();

			ValidateInterestRateAgainstLoanSource();
			ValidateRepaymentPeriodAgainstLoanSource();
			ValidatePeriodSelectionAllowedAgainstLoanSource();
		} // InitLoanSource

		private void ValidateInterestRateAgainstLoanSource() {
			bool interestRateIsGood =
				!this.loanSourceToUse.MaxInterest.HasValue ||
				(this.interestRateToUse <= this.loanSourceToUse.MaxInterest.Value)
			;

			if (interestRateIsGood)
				return;

			Log.Warn(
				"Too big interest ({1}) was assigned for this loan source - adjusting to {2} for customer {0}.",
				CustomerID,
				this.interestRateToUse,
				this.loanSourceToUse.MaxInterest.Value
			);

			this.interestRateToUse = this.loanSourceToUse.MaxInterest.Value;
		} // ValidateInterestRateAgainstLoanSource

		private void ValidateRepaymentPeriodAgainstLoanSource() {
			bool repaymentPeriodIsGood =
				!this.loanSourceToUse.DefaultRepaymentPeriod.HasValue ||
				(this.repaymentPeriodToUse >= this.loanSourceToUse.DefaultRepaymentPeriod);

			if (repaymentPeriodIsGood)
				return;

			Log.Warn(
				"Too small repayment period ({1}) was assigned for this loan source - adjusting to {2} for customer {0}",
				CustomerID,
				this.repaymentPeriodToUse,
				this.loanSourceToUse.DefaultRepaymentPeriod
			);

			this.repaymentPeriodToUse = this.loanSourceToUse.DefaultRepaymentPeriod.Value;
		} // ValidateRepaymentPeriodAgainstLoanSource

		private void ValidatePeriodSelectionAllowedAgainstLoanSource() {
			bool periodSelectionAllowedIsGood =
				this.loanSourceToUse.IsCustomerRepaymentPeriodSelectionAllowed ||
				!this.isCustomerRepaymentPeriodSelectionAllowedToUse;

			if (periodSelectionAllowedIsGood)
				return;

			Log.Warn(
				"Wrong period selection option ('enabled') was assigned for this loan source - " +
				"adjusting to ('disabled') for customer {0}.",
				CustomerID
			);

			this.isCustomerRepaymentPeriodSelectionAllowedToUse = false;
		} // ValidatePeriodSelectionAllowedAgainstLoanSource

		private void InitDiscountPlan() {
			SafeReader dpsr = DB.GetFirst(
				"GetDiscountPlan",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@DiscountPlanID", this.autoDecisionResponse.DiscountPlanID)
			);

			this.discountPlanIDToUse = dpsr["DiscountPlanID"];
		} // InitDiscountPlan

		private void InitLoanType() {
			SafeReader ltsr = DB.GetFirst(
				"GetLoanTypeAndDefault",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@LoanTypeID", this.autoDecisionResponse.LoanTypeID)
			);

			int? dbLoanTypeID = ltsr["LoanTypeID"];
			int defaultLoanTypeID = ltsr["DefaultLoanTypeID"];

			this.loanTypeIDToUse = this.autoDecisionResponse.DecidedToApprove
				? (dbLoanTypeID ?? defaultLoanTypeID)
				: defaultLoanTypeID;
		} // InitLoanType

		private readonly DateTime now;
		private readonly AutoDecisionResponse autoDecisionResponse;
		private readonly LastOfferData lastOffer;
		private readonly int customerID;
		private readonly decimal setupFeePercentToUse;

		private LoanSource loanSourceToUse;
		private decimal interestRateToUse;
		private int repaymentPeriodToUse;
		private bool isCustomerRepaymentPeriodSelectionAllowedToUse;
		private int discountPlanIDToUse;
		private int loanTypeIDToUse;
	} // class MainStrategyUpdateCrC
} // namespace
