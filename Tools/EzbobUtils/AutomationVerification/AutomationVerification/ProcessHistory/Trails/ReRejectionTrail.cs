namespace AutomationCalculator.ProcessHistory.Trails {
	using AutoDecision.AutoReRejection;
	using DbConstants;
	using Ezbob.Logger;

	public class ReRejectionTrail : ATrail {
		#region constructor

		public ReRejectionTrail(int nCustomerID, ASafeLog oLog, string toExplanationMailAddress = null, string fromEmailAddress = null, string fromEmailName = null)
			: base(nCustomerID, DecisionStatus.Affirmative, oLog, toExplanationMailAddress, fromEmailAddress, fromEmailName)
		{
			MyInputData = new ReRejectInputData();
		} // constructor

		#endregion constructor

		#region property Decision

		public override DecisionActions Decision {
			get { return DecisionActions.ReReject; }
		} // Decision

		#endregion property Decision

		#region property Name
		public override string Name { get { return "Auto Re-Reject"; } }
		#endregion property Name

		#region method LockDecision

		public override void LockDecision() {
			if (DecisionStatus != DecisionStatus.Dunno)
				IsDecisionLocked = true;
		} // LockDecision

		#endregion method LockDecision

		#region property InputData

		public override string PositiveDecisionName {
			get { return "re-rejected"; }
		}

		public override string NegativeDecisionName {
			get { return "not re-rejected"; }
		}

		public override ITrailInputData InputData {
			get { return MyInputData; }
		} // InputData

		public virtual ReRejectInputData MyInputData { get; private set; }

		#endregion property InputData

		#region method UpdateDecision

		protected override void UpdateDecision(DecisionStatus nDecisionStatus) {
			if (nDecisionStatus == DecisionStatus.Dunno) {
				return;
			}

			DecisionStatus = nDecisionStatus;
		} // UpdateDecision

		#endregion method UpdateDecision
	} // class ApprovalTrail
} // namespace
