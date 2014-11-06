namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class InitialAssignment : ATrace {
		public InitialAssignment(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		public void Init(int nOfferedCreditLine) {
			OfferedCreditLine = nOfferedCreditLine;

			Comment = string.Format("customer {0} system calculated sum is {1}", CustomerID, OfferedCreditLine);
		} // Init

		public int OfferedCreditLine { get; private set; }
	}  // class InitialAssignment
} // namespace
