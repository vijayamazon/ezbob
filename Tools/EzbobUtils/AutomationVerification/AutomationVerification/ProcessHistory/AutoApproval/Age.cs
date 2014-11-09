namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class Age : ARangeTrace {
		public Age(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ValueName {
			get { return "age"; }
		} // ValueName
	}  // class Age
} // namespace
