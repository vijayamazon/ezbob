namespace AutomationCalculator.ProcessHistory.Trails {
	using AutoDecision.AutoRejection;
	using DbConstants;
	using Ezbob.Logger;

	/// <summary>
	/// Rejection trail contains list of all the rejection steps (traces),
	/// the rejection input model and determines weather customer should be auto rejected or not
	/// and based on which trace the decision was made.
	/// </summary>
	public class RejectionTrail : ATrail {
		/// <summary>
		/// Initial state is no decision is made (if in the end still no decision - then no auto reject is done
		/// </summary>
		public RejectionTrail(
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
			DecisionStatus.Dunno,
			oLog,
			toExplanationMailAddress,
			fromEmailAddress,
			fromEmailName
		) {
			MyInputData = new RejectionInputData();
		} // constructor

		public void Init(RejectionInputData data) {
			MyInputData = data;
		} // Init

		public override string PositiveDecisionName {
			get { return "rejected"; }
		} // PositiveDecisionName

		public override string NegativeDecisionName {
			get { return "not rejected"; }
		} // NegativeDecisionName

		public override string Name { get { return "Auto Reject"; } }

		public override DecisionActions Decision {
			get { return DecisionActions.Reject; }
		} // Decision

		public override void LockDecision() {
			if (DecisionStatus != DecisionStatus.Dunno)
				IsDecisionLocked = true;
		} // LockDecision

		public override ITrailInputData InputData {
			get { return MyInputData; }
		} // InputData

		public virtual RejectionInputData MyInputData { get; private set; }

		public virtual void DecideIfNotDecided() {
			if (DecisionStatus == DecisionStatus.Dunno)
				DecisionStatus = DecisionStatus.Negative;
		} // DecideIfNotDecided

		protected override void UpdateDecision(DecisionStatus nDecisionStatus) {
			if (nDecisionStatus == DecisionStatus.Dunno)
				return;

			DecisionStatus = nDecisionStatus;
		} // UpdateDecision
	} // class ApprovalTrail
} // namespace
