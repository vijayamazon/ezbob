namespace AutomationCalculator.ProcessHistory.Trails {
	using DbConstants;
	using Ezbob.Logger;

	public class RejectionTrail : ATrail {
		#region constructor

		public RejectionTrail(int nCustomerID, ASafeLog oLog) : base (nCustomerID, DecisionStatus.Dunno, oLog) {
		} // constructor

		#endregion constructor

		#region property PositiveDecisionName

		public override string PositiveDecisionName {
			get { return "rejected"; }
		} // PositiveDecisionName

		#endregion property PositiveDecisionName

		#region property NegativeDecisionName

		public override string NegativeDecisionName {
			get { return "not rejected"; }
		} // NegativeDecisionName

		#endregion property NegativeDecisionName

		#region property Decision

		public override DecisionActions Decision {
			get { return DecisionActions.Reject; }
		} // Decision

		#endregion property Decision

		#region method LockDecision

		public override void LockDecision() {
			if (DecisionStatus != DecisionStatus.Dunno)
				IsDecisionLocked = true;
		} // LockDecision

		#endregion method LockDecision

		#region property InputData

		public override ITrailInputData InputData {
			get { return null; } // TODO { return MyInputData; }
		} // InputData

		// public virtual ApprovalInputData MyInputData { get; private set; }

		#endregion property InputData

		#region method UpdateDecision

		protected override void UpdateDecision(DecisionStatus nDecisionStatus) {
			DecisionStatus = DecisionStatus.Dunno;
		} // UpdateDecision

		#endregion method UpdateDecision
	} // class ApprovalTrail
} // namespace
