namespace AutomationCalculator.ProcessHistory.Trails {
	using DbConstants;
	using Ezbob.Logger;

	public class ApprovalTrail : ATrail {
		#region constructor

		public ApprovalTrail(int nCustomerID, ASafeLog oLog, string toExplanationMailAddress = null, string fromEmailAddress = null, string fromEmailName = null)
			: base(nCustomerID, DecisionStatus.Affirmative, oLog, toExplanationMailAddress, fromEmailAddress, fromEmailName)
		{
			MyInputData = new ApprovalInputData();
		} // constructor

		#endregion constructor

		#region property PositiveDecisionName

		public override string PositiveDecisionName {
			get { return "approved"; }
		} // PositiveDecisionName

		#endregion property PositiveDecisionName

		#region property NegativeDecisionName

		public override string NegativeDecisionName {
			get { return "not approved"; }
		} // NegativeDecisionName

		#endregion property NegativeDecisionName

		#region property Name
		public override string Name { get { return "Auto Approve"; } }
		#endregion property Name

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
				DecisionStatus = DecisionStatus.Negative;
		} // UpdateDecision

		#endregion method UpdateDecision
	} // class ApprovalTrail
} // namespace
