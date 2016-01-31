namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;

	internal class Rereject : ADecisionBaseStep {
		public Rereject(
			string outerContextDescription,
			bool avoidAutomaticDecision,
			bool enabled,
			int customerID,
			long cashRequestID,
			long nlCashRequestID,
			string tag
		) : base(
			outerContextDescription,
			avoidAutomaticDecision,
			enabled,
			customerID,
			cashRequestID,
			nlCashRequestID,
			tag
		) {
		} // constructor

		protected override string ProcessName { get { return "auto re-rejection"; } }

		protected override string DecisionName { get { return "rejected"; } }

		protected override IDecisionCheckAgent CreateDecisionCheckAgent() {
			return new ReRejection(CustomerID, CashRequestID, NLCashRequestID, Tag, DB, Log);
		} // CreateDecisionCheckAgent
	} // class Rereject
} // namespace
