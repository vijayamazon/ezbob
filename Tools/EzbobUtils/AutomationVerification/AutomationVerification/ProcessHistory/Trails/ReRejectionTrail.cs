namespace AutomationCalculator.ProcessHistory.Trails {
	using AutoDecision.AutoReRejection;
	using DbConstants;
	using Ezbob.Logger;

	public class ReRejectionTrail : ATrail {
		#region constructor

		public ReRejectionTrail(int nCustomerID, ASafeLog oLog) : base (nCustomerID, DecisionStatus.Dunno, oLog) {
		} // constructor

		#endregion constructor

		#region property DecisionName

		public override string DecisionName {
			get { return "Re-rejection"; }
		} // DecisionName

		#endregion property DecisionName

		#region property Decision

		public override DecisionActions Decision {
			get { return DecisionActions.ReReject; }
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
			get { return MyInputData; }
		} // InputData

		public virtual ReRejectInputData MyInputData { get; private set; }

		#endregion property InputData

		#region method UpdateDecision

		protected override void UpdateDecision(DecisionStatus nDecisionStatus) {
			DecisionStatus = DecisionStatus.Dunno;
		} // UpdateDecision

		#endregion method UpdateDecision
	} // class ApprovalTrail
} // namespace
