namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class AmountOutOfRangle : ARangeTrace {
		public AmountOutOfRangle(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ValueName {
			get { return "calculated amount"; }
		} // ValueName
	}  // class AmountOutOfRangle
} // namespace
