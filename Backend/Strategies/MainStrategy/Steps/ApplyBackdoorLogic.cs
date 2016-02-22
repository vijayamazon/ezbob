namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using EZBob.DatabaseLib.Model.Database;

	internal class ApplyBackdoorLogic : AMainStrategyStep {
		public ApplyBackdoorLogic(
			string outerContextDescription,
			bool backdoorEnabled,
			int customerID,
			string customerEmail,
			bool customerOwnsProperty,
			bool customerIsTest,
			CashRequestOriginator? cashRequestOriginator,
			long cashRequestID,
			long nlCashRequestID,
			int homeOwnerCap,
			int notHomeOwnerCap,
			int smallLoanScenarioLimit,
			bool aspireToMinSetupFee,
			TypeOfBusiness typeOfBusiness,
			int customerOriginID,
			MonthlyRepaymentData requestedLoan,
			int delay,
			int offerValidHours,
			string tag
		) : base(outerContextDescription) {
			this.backdoorEnabled = backdoorEnabled;
			this.customerID = customerID;
			this.customerEmail = customerEmail;
			this.customerOwnsProperty = customerOwnsProperty;
			this.customerIsTest = customerIsTest;
			this.cashRequestOriginator = cashRequestOriginator ?? CashRequestOriginator.Other;
			this.cashRequestID = cashRequestID;
			this.nlCashRequestID = nlCashRequestID;
			this.homeOwnerCap = homeOwnerCap;
			this.notHomeOwnerCap = notHomeOwnerCap;
			this.smallLoanScenarioLimit = smallLoanScenarioLimit;
			this.aspireToMinSetupFee = aspireToMinSetupFee;
			this.typeOfBusiness = typeOfBusiness;
			this.customerOriginID = customerOriginID;
			this.requestedLoan = requestedLoan;
			this.delay = delay;
			this.offerValidHours = offerValidHours;
			this.tag = tag;

			BackdoorLogicApplied = false;

			AutoDecisionResponse = new AutoDecisionResponse(this.customerID);

			Medal = null;
			OfferResult = null;
		} // constructor

		[StepOutput]
		public AutoDecisionResponse AutoDecisionResponse { get; private set; }

		[StepOutput]
		public OfferResult OfferResult { get; private set; }

		[StepOutput]
		public MedalResult Medal { get; private set; }

		[StepOutput]
		public bool BackdoorLogicApplied { get; private set; }

		[StepOutput]
		public int? BackdoorInvestorID { get; set; }

		public override string Outcome {
			get { return BackdoorLogicApplied ? "'applied'" : "'not applied'"; }
		} // Outcome

		protected override StepResults Run() {
			BackdoorInvestorID = null;

			ABackdoorSimpleDetails backdoorSimpleDetails = CreateBackdoor();

			if (backdoorSimpleDetails == null) {
				Log.Debug(
					"Not using back door simple for {0}: back door code from customer email '{1}' " +
					"ain't no matches any existing back door regex.",
					OuterContextDescription,
					this.customerEmail
				);

				return StepResults.NotApplied;
			} // if

			Log.Debug("Using back door simple for {0} as: {1}.", OuterContextDescription, backdoorSimpleDetails);

			backdoorSimpleDetails.SetAdditionalCustomerData(
				this.cashRequestID,
				this.nlCashRequestID,
				this.smallLoanScenarioLimit,
				this.aspireToMinSetupFee,
				this.typeOfBusiness,
				this.customerOriginID,
				this.requestedLoan,
				this.offerValidHours
			);

			if (!backdoorSimpleDetails.SetResult(AutoDecisionResponse))
				return StepResults.NotApplied;

			Medal = backdoorSimpleDetails.Medal;
			OfferResult = backdoorSimpleDetails.OfferResult;

			Medal.SaveToDb(this.cashRequestID, this.nlCashRequestID, this.tag, DB);

			OfferResult.SaveToDb(DB);

			BackdoorLogicApplied = true;

			if (backdoorSimpleDetails.Decision != DecisionActions.Approve)
				return StepResults.Applied;

			BackdoorSimpleApprove bsa = backdoorSimpleDetails as BackdoorSimpleApprove;

			if (bsa == null) { // Should never happen because of the "if" condition.
				BackdoorLogicApplied = false;
				return StepResults.NotApplied;
			} // if

			BackdoorInvestorID = bsa.InvestorID;

			Medal.MedalClassification = bsa.MedalClassification;
			Medal.OfferedLoanAmount = bsa.ApprovedAmount;
			Medal.TotalScoreNormalized = 1m;
			Medal.AnnualTurnover = bsa.ApprovedAmount;

			var glcd = new GetLoanCommissionDefaults(this.cashRequestID, bsa.ApprovedAmount);
			glcd.Execute();

			if (!glcd.IsBrokerCustomer)
				return StepResults.Applied;

			AutoDecisionResponse.BrokerSetupFeePercent = glcd.Result.BrokerCommission;
			AutoDecisionResponse.SetupFee = glcd.Result.ManualSetupFee;

			return StepResults.Applied;
		} // Run

		protected override bool ShouldCollectOutput { get { return BackdoorLogicApplied; } }

		private ABackdoorSimpleDetails CreateBackdoor() {
			if (!this.backdoorEnabled) {
				Log.Debug(
					"Not using back door simple flow for {0}: " +
					"disabled by configuration (BackdoorSimpleAutoDecisionEnabled).",
					OuterContextDescription
				);

				return null;
			} // if

			if (this.cashRequestOriginator != CashRequestOriginator.FinishedWizard) {
				Log.Debug(
					"Not using back door simple flow for {0}: cash request originator is '{1}'.",
					OuterContextDescription,
					this.cashRequestOriginator
				);

				return null;
			} // if

			if (!this.customerIsTest) {
				Log.Debug(
					"Not using back door simple flow for customer {0}: not a test customer.",
					OuterContextDescription
				);

				return null;
			} // if

			return ABackdoorSimpleDetails.Create(
				this.customerID,
				this.customerEmail,
				this.customerOwnsProperty,
				this.requestedLoan.RequestedAmount,
				this.homeOwnerCap,
				this.notHomeOwnerCap,
				this.delay
			);
		} // CreateBackdoor

		private readonly bool backdoorEnabled;
		private readonly int customerID;
		private readonly string customerEmail;
		private readonly bool customerOwnsProperty;
		private readonly bool customerIsTest;
		private readonly CashRequestOriginator cashRequestOriginator;
		private readonly long cashRequestID;
		private readonly long nlCashRequestID;
		private readonly int homeOwnerCap;
		private readonly int notHomeOwnerCap;
		private readonly int smallLoanScenarioLimit;
		private readonly bool aspireToMinSetupFee;
		private readonly TypeOfBusiness typeOfBusiness;
		private readonly int customerOriginID;
		private readonly MonthlyRepaymentData requestedLoan;
		private readonly int delay;
		private readonly int offerValidHours;
		private readonly string tag;
	} // class ApplyBackdoorLogic
} // namespace
