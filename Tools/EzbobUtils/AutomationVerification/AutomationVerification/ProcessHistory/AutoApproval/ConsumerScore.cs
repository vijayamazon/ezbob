namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class ConsumerScore : AThresholdTrace {
		public ConsumerScore(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ValueName {
			get { return "consumer score"; }
		} // ValueName
	}  // class ConsumerScore
} // namespace
