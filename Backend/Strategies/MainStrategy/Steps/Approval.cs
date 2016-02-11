namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.Common;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.MedalCalculations;

	using ApprovalAgent = Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval.LogicalGlue.Agent;

	internal class Approval : AApprovalBaseStep {
		public Approval(
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
			bool autoRerejectionEnabled,
			int proposedAmount,
			MedalResult medal,
			AutoRejectionOutput autoRejectionOutput,
			bool customerIsAlibaba
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
			this.proposedAmount = proposedAmount;
			this.medal = medal;
			this.autoRejectionOutput = autoRejectionOutput;
			this.customerIsAlibaba = customerIsAlibaba;
		} // constructor

		[StepOutput]
		public Guid? AutoApprovalTrailUniqueID {
			get { return this.agent == null ? (Guid?)null : this.agent.Trail.UniqueID; }
		} // AutoApprovalTrailUniqueID

		[StepOutput]
		public bool LoanOfferEmailSendingBannedNew {
			get { return this.agent != null && this.agent.Trail.MyInputData.MetaData.EmailSendingBanned; }
		} // LoanOfferEmailSendingBannedNew

		protected override string ProcessName { get { return "auto approval"; } }

		protected override StepResults Run() {
			if (this.medal == null) {
				Log.Warn(
					"Medal result not specified for {0}, cannot execute {1}, auto-decision process aborted.",
					OuterContextDescription,
					ProcessName
				);

				this.outcome = "'no data'";
				return StepResults.Failed;
			} // if

			if (this.autoRejectionOutput == null) {
				Log.Warn(
					"Auto-rejection output not specified for {0}, cannot execute {1}, auto-decision process aborted.",
					OuterContextDescription,
					ProcessName
				);

				this.outcome = "'no data'";
				return StepResults.Failed;
			} // if

			return base.Run();
		} // Run

		protected override IDecisionCheckAgent CreateDecisionCheckAgent() {
			this.agent = new ApprovalAgent(new AutoApprovalArguments(
				CustomerID,
				CashRequestID,
				NLCashRequestID,
				this.proposedAmount,
				(AutomationCalculator.Common.Medal)this.medal.MedalClassification,
				(AutomationCalculator.Common.MedalType)this.medal.MedalType,
				(AutomationCalculator.Common.TurnoverType?)this.medal.TurnoverType,
				this.autoRejectionOutput.FlowType,
				this.autoRejectionOutput.ErrorInLGData,
				Tag,
				DateTime.UtcNow,
				DB,
				Log
			)).Init();

			return this.agent;
		} // CreateDecisionCheckAgent

		protected override bool PreventAffirmativeDecision() {
			if (base.PreventAffirmativeDecision())
				return true;

			if (!this.customerIsAlibaba)
				return false;

			Log.Msg("Prevented {1} decision for {0}: Alibaba customer.", OuterContextDescription, ProcessName);
			return true;
		} // PreventAffirmativeDecision

		private ApprovalAgent agent;
		private readonly int proposedAmount;
		private readonly MedalResult medal;
		private readonly AutoRejectionOutput autoRejectionOutput;
		private readonly bool customerIsAlibaba;
	} // class Approval
} // namespace
