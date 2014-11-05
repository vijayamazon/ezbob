namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class BusinessScore : ATrace {
		public BusinessScore(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		public virtual ATrace Init(decimal nScore, decimal nThreshold) {
			Score = nScore;
			Threshold = nThreshold;

			Comment = string.Format(
				"customer {0} {3} is {1}, threshold is {2}",
				CustomerID,
				Score,
				Threshold,
				ScoreName
			);

			return this;
		} // Init

		public decimal Score { get; private set; }
		public decimal Threshold { get; private set; }

		protected virtual string ScoreName {
			get { return "business score"; }
		} // ScoreName
	}  // class BusinessScore
} // namespace
