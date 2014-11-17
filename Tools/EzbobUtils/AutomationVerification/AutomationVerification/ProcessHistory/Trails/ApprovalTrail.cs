namespace AutomationCalculator.ProcessHistory.Trails {
	using DbConstants;
	using Ezbob.Logger;

	public class ApprovalTrail : ATrail {
		#region constructor

		public ApprovalTrail(int nCustomerID, ASafeLog oLog) : base (nCustomerID, DecisionStatus.Affirmative, oLog) {
			MyInputData = new ApprovalInputData();
		} // constructor

		#endregion constructor

		#region property DecisionName

		public override string DecisionName {
			get { return "auto approve"; }
		} // DecisionName

		#endregion property DecisionName

		#region property Decision

		public override DecisionActions Decision {
			get { return DecisionActions.Approve; }
		} // Decision

		#endregion property Decision

		#region property InputData

		public override ITrailInputData InputData {
			get { return MyInputData; }
		} // InputData

		public virtual ApprovalInputData MyInputData { get; private set; }

		#endregion property InputData

		#region method UpdateDecision

		protected override void UpdateDecision(DecisionStatus nDecisionStatus) {
			if (nDecisionStatus != DecisionStatus.Affirmative)
				DecisionStatus = DecisionStatus.Dunno;
		} // UpdateDecision

		#endregion method UpdateDecision
	} // class ApprovalTrail
} // namespace
