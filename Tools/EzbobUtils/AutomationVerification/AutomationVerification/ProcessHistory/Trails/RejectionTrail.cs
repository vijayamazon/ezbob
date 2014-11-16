namespace AutomationCalculator.ProcessHistory.Trails {
	using Ezbob.Logger;

	public class RejectionTrail : ATrail {
		#region constructor

		public RejectionTrail(int nCustomerID, ASafeLog oLog) : base (nCustomerID, DecisionStatus.Dunno, oLog) {
		} // constructor

		#endregion constructor

		#region property DecisionName

		public override string DecisionName {
			get { return "auto reject"; }
		} // DecisionName

		#endregion property DecisionName

		#region method LockDecision

		public override void LockDecision() {
			if (DecisionStatus != DecisionStatus.Dunno)
				IsDecisionLocked = true;
		} // LockDecision

		#endregion method LockDecision

		#region method UpdateDecision

		protected override void UpdateDecision(DecisionStatus nDecisionStatus) {
			DecisionStatus = DecisionStatus.Dunno;
		} // UpdateDecision

		#endregion method UpdateDecision
	} // class ApprovalTrail
} // namespace
