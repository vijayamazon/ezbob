namespace Ezbob.Backend.Strategies.MainStrategyNew.Steps {
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.ReApproval;

	internal class Reapproval : ADecisionBaseStep {
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
			tag
		) {
			this.customerStatusIsEnabled = customerStatusIsEnabled;
			this.customerStatusIsWarning = customerStatusIsWarning;
			this.autoRejectionEnabled = autoRejectionEnabled;
			this.autoRerejectionEnabled = autoRerejectionEnabled;
		} // constructor

		[StepOutput]
		public AutoReapprovalOutput AutoReapprovalOutput { get { return this.agent == null ? null : this.agent.Output; } }

		[StepOutput]
		public int ApprovedAmount { get { return this.agent == null ? 0 : this.agent.ApprovedAmount; } }

		protected override string ProcessName { get { return "auto re-approval"; } }

		protected override string DecisionName { get { return "approved"; } }

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

		protected override bool PreventAffirmativeDecision() {
			if (base.PreventAffirmativeDecision())
				return true;

			if (!this.customerStatusIsEnabled) {
				Log.Msg(
					"Preventing {1} decision for {0}: customer status is not enabled.",
					OuterContextDescription,
					ProcessName
				);
				return true;
			} // if

			if (this.customerStatusIsWarning) {
				Log.Msg(
					"Preventing {1} decision for {0}: customer status is 'warning'.",
					OuterContextDescription,
					ProcessName
				);
				return true;
			} // if

			if (!this.autoRerejectionEnabled) {
				Log.Msg(
					"Preventing {1} decision for {0}: auto re-rejections are disabled.",
					OuterContextDescription,
					ProcessName
				);
				return true;
			} // if

			if (!this.autoRejectionEnabled) {
				Log.Msg(
					"Preventing {1} decision for {0}: auto rejections are disabled.",
					OuterContextDescription,
					ProcessName
				);
				return true;
			} // if

			return false;
		} // PreventAffirmativeDecision

		private AutoDecisionAutomation.AutoDecisions.ReApproval.Agent agent;

		private readonly bool customerStatusIsEnabled;
		private readonly bool customerStatusIsWarning;
		private readonly bool autoRejectionEnabled;
		private readonly bool autoRerejectionEnabled;
	} // class Reapproval
} // namespace
