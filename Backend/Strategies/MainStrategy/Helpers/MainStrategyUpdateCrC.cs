namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
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
			DateTime now,
			int customerID,
			long cashRequestID,
			AutoDecisionResponse autoDecisionResponse,
			AConnection db,
			ASafeLog log
		) : base(db, log) {
			this.now = now;

			this.customerID = customerID;
			CashRequestID = cashRequestID;
			this.autoDecisionResponse = autoDecisionResponse;

			this.loanSourceToUse = this.autoDecisionResponse.LoanSource;

			this.repaymentPeriodToUse = this.autoDecisionResponse.RepaymentPeriod;

			this.isCustomerRepaymentPeriodSelectionAllowedToUse = this.loanSourceToUse.ValidatePeriodSelectionAllowed(
				this.autoDecisionResponse.IsCustomerRepaymentPeriodSelectionAllowed
			);

			if (this.autoDecisionResponse.DecidedToApprove)
				this.interestRateToUse = this.loanSourceToUse.ValidateInterestRate(this.autoDecisionResponse.InterestRate);

			this.repaymentPeriodToUse = this.loanSourceToUse.ValidateRepaymentPeriod(this.repaymentPeriodToUse);
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
			get {
				bool hasGoodValidFor =
					this.autoDecisionResponse.AppValidFor.HasValue &&
					(this.autoDecisionResponse.AppValidFor.Value > Now);

				return hasGoodValidFor
					? this.autoDecisionResponse.AppValidFor.Value
					: Now.AddHours(CurrentValues.Instance.OfferValidForHours);
			} // get
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
			get { return this.autoDecisionResponse.IsLoanTypeSelectionAllowed; }
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
			get { return this.autoDecisionResponse.LoanTypeID; }
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
			get { return this.autoDecisionResponse.SetupFee; }
			set { }
		} // ManualSetupFeePercent

		public decimal? BrokerSetupFeePercent {
			get { return this.autoDecisionResponse.BrokerSetupFeePercent; }
			set { }
		} // BrokerSetupFeePercent

		public bool SpreadSetupFee {
			get { return this.autoDecisionResponse.SpreadSetupFee; }
			set { }
		} // SpreadSetupFee

		public bool IsCustomerRepaymentPeriodSelectionAllowed {
			get { return this.isCustomerRepaymentPeriodSelectionAllowedToUse; }
			set { }
		} // IsCustomerRepaymentPeriodSelectionAllowed

		public bool EmailSendingBanned {
			get { return this.autoDecisionResponse.LoanOfferEmailSendingBannedNew; }
			set { }
		} // EmailSendingBanned

		public int DiscountPlanID {
			get { return this.autoDecisionResponse.DiscountPlanIDToUse; }
			set { }
		} // DiscountPlanID

		public bool HasApprovalChance {
			get { return this.autoDecisionResponse.HasApprovalChance; }
			set { }
		} // HasApprovalChance

		public int? ProductSubTypeID {
			get { return this.autoDecisionResponse.ProductSubTypeID; }
			set { }
		} // ProductSubTypeID

		// Stored procedure arguments - end

		private readonly DateTime now;
		private readonly AutoDecisionResponse autoDecisionResponse;
		private readonly int customerID;
		private readonly LoanSource loanSourceToUse;
		private readonly decimal interestRateToUse;
		private readonly int repaymentPeriodToUse;
		private readonly bool isCustomerRepaymentPeriodSelectionAllowedToUse;
	} // class MainStrategyUpdateCrC
} // namespace
