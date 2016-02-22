namespace AutomationCalculator.ProcessHistory.Trails {
	using AutomationCalculator.ProcessHistory.Trails.ApprovalInput;
	using DbConstants;
	using Ezbob.Logger;

	public class ApprovalTrail : ATrail {
		public ApprovalTrail(
			int nCustomerID,
			long? cashRequestID,
			long? nlCashRequestID,
			ASafeLog oLog,
			string toExplanationMailAddress = null,
			string fromEmailAddress = null,
			string fromEmailName = null
		) : base(
			nCustomerID,
			cashRequestID,
			nlCashRequestID,
			DecisionStatus.Affirmative,
			oLog,
			toExplanationMailAddress,
			fromEmailAddress,
			fromEmailName
		) {
			HasApprovalChance = true;
		} // constructor

		public override string PositiveDecisionName {
			get { return "approved"; }
		} // PositiveDecisionName

		public override string NegativeDecisionName {
			get { return "not approved"; }
		} // NegativeDecisionName

		public override string Name { get { return "Auto Approve"; } }

		public override DecisionActions Decision {
			get { return DecisionActions.Approve; }
		} // Decision

		public override ITrailInputData InputData {
			get { return MyInputData; }
		} // InputData

		public virtual ApprovalInputData MyInputData {
			get {
				if (this.inputData == null)
					this.inputData = CreateInputData();

					return this.inputData;
			} // get
		} // MyInputData

		protected virtual ApprovalInputData CreateInputData() {
			return new ApprovalInputData();
		} // CreateInputData

		protected override void UpdateDecision(DecisionStatus nDecisionStatus) {
			if (nDecisionStatus != DecisionStatus.Affirmative)
				DecisionStatus = DecisionStatus.Negative;
		} // UpdateDecision

		private ApprovalInputData inputData;
	} // class ApprovalTrail
} // namespace
