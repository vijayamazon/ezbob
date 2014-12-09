namespace AutomationCalculator.ProcessHistory.Trails {
	using AutoDecision.AutoReRejection;
	using DbConstants;
	using Ezbob.Logger;

	public class ReRejectionTrail : ATrail {

		public ReRejectionTrail(int nCustomerID, ASafeLog oLog, string toExplanationMailAddress = null, string fromEmailAddress = null, string fromEmailName = null)
			: base(nCustomerID, DecisionStatus.Affirmative, oLog, toExplanationMailAddress, fromEmailAddress, fromEmailName)
		{
			MyInputData = new ReRejectInputData();
		} // constructor

		public override DecisionActions Decision {
			get { return DecisionActions.ReReject; }
		} // Decision

		public override string Name { get { return "Auto Re-Reject"; } }

		public override void LockDecision() {
			if (DecisionStatus != DecisionStatus.Dunno)
				IsDecisionLocked = true;
		} // LockDecision

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

		protected override void UpdateDecision(DecisionStatus nDecisionStatus) {
			if (nDecisionStatus == DecisionStatus.Dunno) {
				return;
			}

			DecisionStatus = nDecisionStatus;
		} // UpdateDecision

	} // class ApprovalTrail
} // namespace
