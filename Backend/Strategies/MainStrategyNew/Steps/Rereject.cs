namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;

	internal class Rereject : ADecisionBaseStep {
		public Rereject(
			string outerContextDescription,
			AMainStrategyStep onRejected,
			AMainStrategyStep onNotRejected,
			AMainStrategyStep onFailure,
			bool avoidAutomaticDecision,
			bool enabled,
			int customerID,
			long cashRequestID,
			long nlCashRequestID,
			string tag
		) : base(
			outerContextDescription,
			onRejected,
			onNotRejected,
			onFailure,
			avoidAutomaticDecision,
			enabled,
			customerID,
			cashRequestID,
			nlCashRequestID,
			tag
		) {
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
					"Not processing auto-rejections for {0}: auto re-rejection is disabled.",
					OuterContextDescription
				);

				this.outcome = "'not rejected'";
				return OnNotDecided;
			} // if

			ReRejection rrAgent = null;

			try {
				rrAgent = new ReRejection(CustomerID, CashRequestID, NLCashRequestID, DB, Log);
				rrAgent.MakeAndVerifyDecision(Tag);
			} catch (Exception e) {
				Log.Alert(
					e,
					"Uncaught exception during re-rejection for {0}, auto-decision process aborted.",
					OuterContextDescription
				);

				this.outcome = "'uncaught exception'";
				return OnFailure;
			} // try

			if (rrAgent.ExceptionDuringRerejection) {
				Log.Warn(
					"Exception happened while executing re-rejection for {0}, auto-decision process aborted.",
					OuterContextDescription
				);

				this.outcome = "'exception'";
				return OnFailure;
			} // if

			if (rrAgent.WasMismatch) {
				Log.Warn(
					"Mismatch happened while executing re-rejection for {0}, auto-decision process aborted.",
					OuterContextDescription
				);

				this.outcome = "'mismatch'";
				return OnFailure;
			} // if

			if (rrAgent.Trail.HasDecided) {
				Log.Warn("Re-rejection for {0} decided to reject.", OuterContextDescription);

				this.outcome = "'rejected'";
				return OnDecided;
			} // if

			Log.Warn("Re-rejection for {0} decided not to reject.", OuterContextDescription);

			this.outcome = "'not rejected'";
			return OnNotDecided;
		} // Run
	} // class Rereject
} // namespace
