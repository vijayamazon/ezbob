namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;

	internal abstract class ADecisionBaseStep : AMainStrategyStep {
		protected ADecisionBaseStep(
			string outerContextDescription,
			bool avoidAutomaticDecision,
			bool enabled,
			int customerID,
			long cashRequestID,
			long nlCashRequestID,
			string tag
		) : base(outerContextDescription) {
			AvoidAutomaticDecision = avoidAutomaticDecision;
			Enabled = enabled;
			CustomerID = customerID;
			CashRequestID = cashRequestID;
			NLCashRequestID = nlCashRequestID;
			Tag = tag;
			this.outcome = "'not done'";
		} // constructor

		public override string Outcome { get { return this.outcome; } }

		protected bool AvoidAutomaticDecision { get; private set; }
		protected bool Enabled { get; private set; }
		protected int CustomerID { get; private set; }
		protected long CashRequestID { get; private set; }
		protected long NLCashRequestID { get; private set; }
		protected string Tag { get; private set; }

		protected abstract string ProcessName { get; } // like 'auto rejection', 'auto re-rejection', 'auto approval'

		protected abstract string DecisionName { get; } // like 'approved', 'rejected'

		protected abstract IDecisionCheckAgent CreateDecisionCheckAgent();

		protected virtual bool PreventAffirmativeDecision() {
			if (AvoidAutomaticDecision) {
				Log.Msg(
					"Preventing {1} decision for {0}: auto decisions should be avoided.",
					OuterContextDescription,
					ProcessName
				);

				return true;
			} // if

			if (!Enabled) {
				Log.Msg(
					"Preventing {1} decision for {0}: {1} is disabled.",
					OuterContextDescription,
					ProcessName
				);

				return true;
			} // if

			return false;
		} // PreventAffirmativeDecision

		protected override StepResults Run() {
			IDecisionCheckAgent agent;

			try {
				agent = CreateDecisionCheckAgent();
				agent.MakeAndVerifyDecision();
			} catch (Exception e) {
				Log.Alert(
					e,
					"Uncaught exception during {1} for {0}, auto-decision process aborted.",
					OuterContextDescription,
					ProcessName
				);

				this.outcome = "'uncaught exception'";
				return StepResults.Failed;
			} // try

			if (agent.WasException) {
				Log.Warn(
					"Exception happened while executing {1} for {0}, auto-decision process aborted.",
					OuterContextDescription,
					ProcessName
				);

				this.outcome = "'exception'";
				return StepResults.Failed;
			} // if

			if (agent.WasMismatch) {
				Log.Warn(
					"Mismatch happened while executing {1} for {0}, auto-decision process aborted.",
					OuterContextDescription,
					ProcessName
				);

				this.outcome = "'mismatch'";
				return StepResults.Failed;
			} // if

			StepResults result;

			if (agent.AffirmativeDecisionMade && !PreventAffirmativeDecision()) {
				this.outcome = string.Format("'{0}'", DecisionName);
				result = StepResults.Affirmative;
			} else {
				this.outcome = string.Format("'not {0}'", DecisionName);
				result = StepResults.Negative;
			} // if

			Log.Msg("Process of {1} for {0} decided {2}.", OuterContextDescription, ProcessName, Outcome);

			return result;
		} // Run

		protected string outcome;
	} // class ADecisionBaseStep
} // namespace
