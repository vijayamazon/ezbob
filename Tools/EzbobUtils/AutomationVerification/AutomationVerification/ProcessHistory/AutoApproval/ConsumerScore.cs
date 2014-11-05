namespace AutomationCalculator.ProcessHistory.AutoApproval {
	public class ConsumerScore : BusinessScore {
		public ConsumerScore(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		protected override string ScoreName {
			get { return "consumer score"; }
		} // ScoreName
	}  // class ConsumerScore
} // namespace
