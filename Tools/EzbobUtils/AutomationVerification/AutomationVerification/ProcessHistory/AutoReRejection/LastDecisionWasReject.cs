namespace AutomationCalculator.ProcessHistory.AutoReRejection
{
	public class LastDecisionWasReject : ATrace {
		public LastDecisionWasReject(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { } // constructor

		public void Init(bool lastDecisionWasReject) {
			Comment = string.Format("Customer's last decision was {0}reject", lastDecisionWasReject ? string.Empty : "not ");
		}
	}  // class 
} // namespace
