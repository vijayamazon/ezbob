namespace AutomationCalculator.ProcessHistory.Trails
{
	using AutoDecision.AutoRejection;
	using DbConstants;
	using Ezbob.Logger;

	/// <summary>
	/// Rejection trail contains list of all the rejection steps (traces),
	/// the rejection input model and determines weather customer should be auto rejected or not and based on which trace the decision was made
	/// </summary>
	public class RejectionTrail : ATrail
	{
		#region constructor

		/// <summary>
		/// Initial state is no decision is made (if in the end still no decision - then no auto reject is done
		/// </summary>
		/// <param name="nCustomerID"></param>
		/// <param name="oLog"></param>
		public RejectionTrail(int nCustomerID, ASafeLog oLog)
			: base(nCustomerID, DecisionStatus.Dunno, oLog)
		{
			MyInputData = new RejectionInputData();

		} // constructor

		#endregion constructor

		#region init data
		public void Init(RejectionInputData data) {
			MyInputData = data;
		}
		#endregion
		#region property PositiveDecisionName

		public override string PositiveDecisionName
		{
			get { return "rejected"; }
		} // PositiveDecisionName

		#endregion property PositiveDecisionName

		#region property NegativeDecisionName

		public override string NegativeDecisionName
		{
			get { return "not rejected"; }
		} // NegativeDecisionName

		#endregion property NegativeDecisionName

		#region property Decision

		public override DecisionActions Decision
		{
			get { return DecisionActions.Reject; }
		} // Decision

		#endregion property Decision

		#region method LockDecision

		public override void LockDecision()
		{
			if (DecisionStatus != DecisionStatus.Dunno)
				IsDecisionLocked = true;
		} // LockDecision

		#endregion method LockDecision

		#region property InputData

		public override ITrailInputData InputData
		{
			get { return MyInputData; }
		} // InputData

		public virtual RejectionInputData MyInputData { get; private set; }

		#endregion property InputData

		#region method UpdateDecision

		protected override void UpdateDecision(DecisionStatus nDecisionStatus)
		{
			DecisionStatus = nDecisionStatus;
		} // UpdateDecision

		#endregion method UpdateDecision
	} // class ApprovalTrail
} // namespace
