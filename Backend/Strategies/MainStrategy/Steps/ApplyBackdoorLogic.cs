namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.Misc;
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
			this.tag = tag;

			this.applied = false;

			AutoDecisionResponse = new AutoDecisionResponse(this.customerID);
		} // constructor

		[StepOutput]
		public AutoDecisionResponse AutoDecisionResponse { get; private set; }

		[StepOutput]
		public MedalResult Medal { get; private set; }

		protected override string Outcome {
			get { return this.applied ? "'applied'" : "'not applied'"; }
		} // Outcome

		protected override StepResults Run() {
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

			bool success = backdoorSimpleDetails.SetResult(AutoDecisionResponse);

			if (!success)
				return StepResults.NotApplied;

			Medal = CalculateMedal();

			this.applied = true;

			if (backdoorSimpleDetails.Decision != DecisionActions.Approve)
				return StepResults.Applied;

			BackdoorSimpleApprove bsa = backdoorSimpleDetails as BackdoorSimpleApprove;

			if (bsa == null) { // Should never happen because of the "if" condition.
				this.applied = false;
				return StepResults.NotApplied;
			} // if

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

		protected override bool ShouldCollectOutput { get { return this.applied; } }

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
				this.customerOwnsProperty
			);
		} // CreateBackdoor

		private MedalResult CalculateMedal() {
			var instance = new CalculateMedal(
				this.customerID,
				this.cashRequestID,
				this.nlCashRequestID,
				DateTime.UtcNow,
				false,
				true
			) {
				Tag = this.tag,
			};
			instance.Execute();

			return instance.Result;
		} // CalculateMedal

		private readonly bool backdoorEnabled;
		private readonly int customerID;
		private readonly string customerEmail;
		private readonly bool customerOwnsProperty;
		private readonly bool customerIsTest;
		private readonly CashRequestOriginator cashRequestOriginator;
		private readonly long cashRequestID;
		private readonly long nlCashRequestID;
		private readonly string tag;

		private bool applied;
	} // class ApplyBackdoorLogic
} // namespace
