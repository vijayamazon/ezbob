namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using Ezbob.Utils.Lingvo;

	public class TotalLoanCount : ATrace {
		#region constructor

		public TotalLoanCount(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		#endregion constructor

		#region method Init

		public void Init(int nLoanCount) {
			LoanCount = nLoanCount;

			Comment = string.Format("customer {0} has {1}", CustomerID, Grammar.Number(LoanCount, "loan"));
		} // Init

		#endregion method Init

		public int LoanCount { get; private set; }
	} // class TotalLoanCount
} // namespace
