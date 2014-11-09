namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class OutstandingOffers : AThresholdTrace {
		public OutstandingOffers(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ValueName {
			get { return "outstanding offers"; }
		} // ValueName
	}  // class OutstandingOffers
} // namespace
