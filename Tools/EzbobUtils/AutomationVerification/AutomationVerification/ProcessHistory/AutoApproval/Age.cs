namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class Age : AmountOutOfRangle {
		public Age(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ValueName {
			get { return "age"; }
		} // ValueName
	}  // class Age
} // namespace
