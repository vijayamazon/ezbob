namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using AutomationCalculator.Common;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using RejectAgent = Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject.LogicalGlue.Agent;

	internal class Reject : ADecisionBaseStep {
		public Reject(
			string outerContextDescription,
			bool avoidAutomaticDecision,
			bool enabled,
			int customerID,
			long cashRequestID,
			long nlCashRequestID,
			string tag,
			int companyID,
			bool customerIsAlibaba
		) : base(
			outerContextDescription,
			avoidAutomaticDecision,
			enabled,
			customerID,
			cashRequestID,
			nlCashRequestID,
			tag
		) {
			this.companyID = companyID;
			this.customerIsAlibaba = customerIsAlibaba;
		} // constructor

		[StepOutput]
		public AutoRejectionOutput AutoRejectionOutput {
			get { return this.rejectAgent == null ? null : this.rejectAgent.Output; }
		} // AutoRejectionOutput

		protected override string ProcessName { get { return "auto rejection"; } }

		protected override string DecisionName { get { return "rejected"; } }

		protected override IDecisionCheckAgent CreateDecisionCheckAgent() {
			this.rejectAgent = new RejectAgent(
				new AutoRejectionArguments(
					CustomerID,
					this.companyID,
					CashRequestID,
					NLCashRequestID,
					Tag,
					DateTime.UtcNow,
					DB,
					Log
				)
			);

			return this.rejectAgent;
		} // CreateDecisionCheckAgent

		protected override bool PreventAffirmativeDecision() {
			if (base.PreventAffirmativeDecision())
				return true;

			if (!this.customerIsAlibaba)
				return false;

			Log.Msg("Prevented {1} decision for {0}: Alibaba customer.", OuterContextDescription, ProcessName);
			return true;
		} // PreventAffirmativeDecision

		private RejectAgent rejectAgent;
		private readonly int companyID;
		private readonly bool customerIsAlibaba;
	} // class Reject
} // namespace
