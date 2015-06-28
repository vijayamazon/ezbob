namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.LoadAndCategorize {
	using DbConstants;

	public class ManualDecision : ADecision {
		public long CashRequestID { get; set; }

		public bool IsOldCustomer { get; set; }
		public bool HasSignature { get; set; }

		public bool AmlChecked { get; set; }
		public bool ConsumerChecked { get; set; }
		public bool CompanyChecked { get; set; }
		public bool MpUpdated { get; set; }

		public bool HasEnoughData {
			get { return !AmlChecked && !ConsumerChecked && !CompanyChecked && !MpUpdated; }
		} // HasEnoughData

		public int ManualDecisionID {
			get { return ApproveStatus == ApproveStatus.Yes ? (int)DecisionActions.Approve : (int)DecisionActions.Reject; }
		} // ManualDecisionID
	} // class ManualDecision
} // namespace
