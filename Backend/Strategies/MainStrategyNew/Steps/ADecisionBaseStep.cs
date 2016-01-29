namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using System;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;

	internal abstract class ADecisionBaseStep : AThreeExitStep {
		protected ADecisionBaseStep(
			string outerContextDescription,
			AMainStrategyStep onDecided,
			AMainStrategyStep onNotDecided,
			AMainStrategyStep onFailure,
			bool avoidAutomaticDecision,
			bool enabled,
			int customerID,
			long cashRequestID,
			long nlCashRequestID,
			string tag
		) : base(outerContextDescription, onDecided, onNotDecided, onFailure) {
			AvoidAutomaticDecision = avoidAutomaticDecision;
			Enabled = enabled;
			CustomerID = customerID;
			CashRequestID = cashRequestID;
			NLCashRequestID = nlCashRequestID;
			Tag = tag;
			this.outcome = "'not done'";
		} // constructor

		protected override string Outcome { get { return this.outcome; } }

		protected AMainStrategyStep OnDecided { get { return FirstExit; } }
		protected AMainStrategyStep OnNotDecided { get { return SecondExit; } }
		protected AMainStrategyStep OnFailure { get { return ThirdExit; } }

		protected bool AvoidAutomaticDecision { get; private set; }
		protected bool Enabled { get; private set; }
		protected int CustomerID { get; private set; }
		protected long CashRequestID { get; private set; }
		protected long NLCashRequestID { get; private set; }
		protected string Tag { get; private set; }

		protected abstract string ProcessName { get; } // like 'auto rejection', 'auto re-rejection', 'auto approval'

		protected abstract string DecisionName { get; } // like 'approved', 'rejected'

		protected abstract IDecisionCheckAgent CreateDecisionCheckAgent();

		protected virtual AMainStrategyStep CheckCustomPreventers() {
			return null;
		} // CheckCustomPreventers

		protected override AMainStrategyStepBase Run() {
			if (AvoidAutomaticDecision) {
				Log.Msg(
					"Not processing {1} for {0}: auto decisions should be avoided.",
					OuterContextDescription,
					ProcessName
				);

				this.outcome = string.Format("'not {0}'", DecisionName);
				return OnNotDecided;
			} // if

			if (!Enabled) {
				Log.Msg(
					"Not processing {1} for {0}: {1} is disabled.",
					OuterContextDescription,
					ProcessName
				);

				this.outcome = string.Format("'not {0}'", DecisionName);
				return OnNotDecided;
			} // if

			AMainStrategyStep customPreventerExit = CheckCustomPreventers();

			if (customPreventerExit != null)
				return customPreventerExit;

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
				return OnFailure;
			} // try

			if (agent.WasException) {
				Log.Warn(
					"Exception happened while executing {1} for {0}, auto-decision process aborted.",
					OuterContextDescription,
					ProcessName
				);

				this.outcome = "'exception'";
				return OnFailure;
			} // if

			if (agent.WasMismatch) {
				Log.Warn(
					"Mismatch happened while executing {1} for {0}, auto-decision process aborted.",
					OuterContextDescription,
					ProcessName
				);

				this.outcome = "'mismatch'";
				return OnFailure;
			} // if

			AMainStrategyStep result;

			if (agent.AffirmativeDecisionMade) {
				this.outcome = string.Format("'{0}'", DecisionName);
				result = OnDecided;
			} else {
				this.outcome = string.Format("'not {0}'", DecisionName);
				result = OnNotDecided;
			} // if

			Log.Msg("Process of {1} for {0} decided {2}.", OuterContextDescription, ProcessName, Outcome);

			return result;
		} // Run

		protected string outcome;
	} // class ADecisionBaseStep
} // namespace
