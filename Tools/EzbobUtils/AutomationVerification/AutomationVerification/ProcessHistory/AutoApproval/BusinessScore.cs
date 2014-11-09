namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class BusinessScore : AThresholdTrace {
		public BusinessScore(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ValueName {
			get { return "business score"; }
		} // ValueName
	}  // class BusinessScore
} // namespace
