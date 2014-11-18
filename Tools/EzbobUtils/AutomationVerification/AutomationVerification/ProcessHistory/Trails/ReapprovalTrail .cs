namespace AutomationCalculator.ProcessHistory.Trails {
	using AutoDecision.AutoReApproval;
	using DbConstants;
	using Ezbob.Logger;

	public class ReapprovalTrail : ATrail {
		#region constructor

		public ReapprovalTrail(int nCustomerID, ASafeLog oLog) : base (nCustomerID, DecisionStatus.Affirmative, oLog) {
			MyInputData = new ReApprovalInputData();
		} // constructor

		#endregion constructor

		#region property DecisionName

		public override string DecisionName {
			get { return "auto re-approve"; }
		} // DecisionName

		#endregion property DecisionName

		#region property Decision

		public override DecisionActions Decision {
			get { return DecisionActions.ReApprove; }
		} // Decision

		#endregion property Decision

		#region property InputData

		public override ITrailInputData InputData {
			get { return MyInputData; }
		} // InputData

		public virtual ReApprovalInputData MyInputData { get; private set; }

		#endregion property InputData

		#region method UpdateDecision

		protected override void UpdateDecision(DecisionStatus nDecisionStatus) {
			if (nDecisionStatus != DecisionStatus.Affirmative)
				DecisionStatus = DecisionStatus.Dunno;
		} // UpdateDecision

		#endregion method UpdateDecision
	} // class ApprovalTrail
} // namespace
