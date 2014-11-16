namespace AutomationCalculator.ProcessHistory.Trails {
	using Ezbob.Logger;

	public class ApprovalTrail : ATrail {
		#region constructor

		public ApprovalTrail(int nCustomerID, ASafeLog oLog) : base (nCustomerID, DecisionStatus.Affirmative, oLog) {
		} // constructor

		#endregion constructor

		#region property DecisionName

		public override string DecisionName {
			get { return "auto approve"; }
		} // DecisionName

		#endregion property DecisionName

		#region method UpdateDecision

		protected override void UpdateDecision(DecisionStatus nDecisionStatus) {
			if (nDecisionStatus != DecisionStatus.Affirmative)
				DecisionStatus = DecisionStatus.Dunno;
		} // UpdateDecision

		#endregion method UpdateDecision
	} // class ApprovalTrail
} // namespace
