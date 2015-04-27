namespace Ezbob.Backend.Strategies.AutomationVerification.KPMG {
	using AutomationCalculator.ProcessHistory.Trails;
	using DbConstants;

	public class AutomationTrails {
		public AutomationTrails() {
			AutomationDecision = DecisionActions.Waiting;
		} // constructor

		public DecisionActions AutomationDecision { get; set; }

		public ApprovalTrail Approval { get; set; }

		public RejectionTrail Rejection { get; set; }
	} // class AutomationTrails
} // namespace
