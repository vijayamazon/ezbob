namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class Rollovers : ABoolTrace {
		public Rollovers(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string PropertyName {
			get { return "rollovers"; }
		} // PropertyName
	}  // class Rollovers
} // namespace
