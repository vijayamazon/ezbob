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

		protected string outcome;
	} // class ADecisionBaseStep
} // namespace
