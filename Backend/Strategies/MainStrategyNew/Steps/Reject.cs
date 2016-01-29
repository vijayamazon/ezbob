namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using RejectAgent = Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.LogicalGlue.Agent;

	internal class Reject : ADecisionBaseStep {
		public Reject(
			string outerContextDescription,
			AMainStrategyStep onDecided,
			AMainStrategyStep onNotDecided,
			AMainStrategyStep onFailure,
			bool avoidAutomaticDecision,
			bool enabled,
			int customerID,
			long cashRequestID,
			long nlCashRequestID,
			string tag,
			int companyID,
			int monthlyPayment
		) : base(
			outerContextDescription,
			onDecided,
			onNotDecided,
			onFailure,
			avoidAutomaticDecision,
			enabled,
			customerID,
			cashRequestID,
			nlCashRequestID,
			tag
		) {
			this.companyID = companyID;
			this.monthlyPayment = monthlyPayment;
		} // constructor

		protected override AMainStrategyStepBase Run() {
			if (AvoidAutomaticDecision) {
				Log.Msg(
					"Not processing auto-rejections for {0}: auto decisions should be avoided.",
					OuterContextDescription
				);

				this.outcome = "'not rejected'";
				return OnNotDecided;
			} // if

			if (!Enabled) {
				Log.Msg(
					"Not processing auto-rejections for {0}: auto rejection is disabled.",
					OuterContextDescription
				);

				this.outcome = "'not rejected'";
				return OnNotDecided;
			} // if

			RejectAgent rAgent;

			try {
				rAgent = new RejectAgent(
					new AutoRejectionArguments(
						CustomerID,
						this.companyID,
						this.monthlyPayment,
						CashRequestID,
						NLCashRequestID,
						Tag,
						DateTime.UtcNow,
						DB,
						Log
					)
				);

				rAgent.MakeAndVerifyDecision();
			} catch (Exception e) {
				Log.Alert(
					e,
					"Uncaught exception during rejection for {0}, auto-decision process aborted.",
					OuterContextDescription
				);

				this.outcome = "'uncaught exception'";
				return OnFailure;
			} // try

			if (rAgent.WasException) {
				Log.Warn(
					"Exception happened while executing rejection for {0}, auto-decision process aborted.",
					OuterContextDescription
				);

				this.outcome = "'exception'";
				return OnFailure;
			} // if

			if (rAgent.WasMismatch) {
				Log.Warn(
					"Mismatch happened while executing rejection for {0}, auto-decision process aborted.",
					OuterContextDescription
				);

				this.outcome = "'mismatch'";
				return OnFailure;
			} // if

			if (rAgent.AffirmativeDecisionMade) {
				Log.Warn("Rejection for {0} decided to reject.", OuterContextDescription);

				this.outcome = "'rejected'";
				return OnDecided;
			} // if

			Log.Warn("Rejection for {0} decided not to reject.", OuterContextDescription);

			this.outcome = "'not rejected'";
			return OnNotDecided;
		} // Run

		private readonly int companyID;
		private readonly int monthlyPayment;
	} // class Reject
} // namespace
