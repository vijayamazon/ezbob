namespace AutomationCalculator.ProcessHistory.ReApproval {
	public class LateLoans : ABoolTrace {
		public LateLoans(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string PropertyName {
			get { return "late loans"; }
		} // PropertyName
	} // class WorstCaisStatus
} // namespace