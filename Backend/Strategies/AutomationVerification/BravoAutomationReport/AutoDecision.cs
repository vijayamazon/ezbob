namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport {
	using System;
	using DbConstants;

	internal class AutoDecision : ADecision {
		public AutoDecision(DecisionActions? decision, Guid? trailUniqueID = null, int? runTimeCount = null) {
			DecisionTime = DateTime.UtcNow;

			TrailUniqueID = trailUniqueID;

			if (decision == null) {
				ApproveStatus = ApproveStatus.Dunno;
				AutoDecisionID = (int)DecisionActions.Waiting;
				return;
			} // if

			if (decision.Value.In(DecisionActions.Approve, DecisionActions.ReApprove))
				ApproveStatus = ApproveStatus.Yes;
			else if (decision.Value.In(DecisionActions.Reject, DecisionActions.ReReject))
				ApproveStatus = ApproveStatus.No;
			else
				ApproveStatus = ApproveStatus.Dunno;

			AutoDecisionID = (int)decision.Value;
		} // constructor

		public Guid? TrailUniqueID { get; private set; }
	} // class AutoDecision
} // namespace
