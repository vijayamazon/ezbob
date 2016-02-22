namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval;

	internal class Reapproval : AApprovalBaseStep {
		public Reapproval(
			string outerContextDescription,
			bool avoidAutomaticDecision,
			bool enabled,
			int customerID,
			long cashRequestID,
			long nlCashRequestID,
			string tag,
			bool customerStatusIsEnabled,
			bool customerStatusIsWarning,
			bool autoRejectionEnabled,
			bool autoRerejectionEnabled
		) : base(
			outerContextDescription,
			avoidAutomaticDecision,
			enabled,
			customerID,
			cashRequestID,
			nlCashRequestID,
			tag,
			customerStatusIsEnabled,
			customerStatusIsWarning,
			autoRejectionEnabled,
			autoRerejectionEnabled
		) {
		} // constructor

		[StepOutput]
		public AutoReapprovalOutput AutoReapprovalOutput { get { return this.agent == null ? null : this.agent.Output; } }

		[StepOutput]
		public int ApprovedAmount { get { return this.agent == null ? 0 : this.agent.ApprovedAmount; } }

		protected override string ProcessName { get { return "auto re-approval"; } }

		protected override IDecisionCheckAgent CreateDecisionCheckAgent() {
			this.agent = new AutoDecisionAutomation.AutoDecisions.ReApproval.Agent(
				CustomerID,
				CashRequestID,
				NLCashRequestID,
				Tag,
				DB,
				Log
			).Init();

			return this.agent;
		} // CreateDecisionCheckAgent

		private AutoDecisionAutomation.AutoDecisions.ReApproval.Agent agent;
	} // class Reapproval
} // namespace
