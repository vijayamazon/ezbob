namespace AutomationCalculator.ProcessHistory.Trails {
	using AutomationCalculator.ProcessHistory.Trails.ApprovalInput;
	using Ezbob.Logger;

	public class LGApprovalTrail : ApprovalTrail {
		public LGApprovalTrail(
			int customerID,
			long? cashRequestID,
			long? nlCashRequestID,
			ASafeLog log,
			string toExplanationMailAddress = null,
			string fromEmailAddress = null,
			string fromEmailName = null
		) : base(
			customerID,
			cashRequestID,
			nlCashRequestID,
			log,
			toExplanationMailAddress,
			fromEmailAddress,
			fromEmailName
		) {
		} // constructor

		public virtual new LGApprovalInputData MyInputData {
			get { return (LGApprovalInputData)base.MyInputData; }
		} // MyInputData

		protected override ApprovalInputData CreateInputData() {
			return new LGApprovalInputData();
		} // CreateInputData
	} // class LGApprovalTrail
} // namespace
