namespace AutomationCalculator.ProcessHistory.AutoApproval {
	using Ezbob.Utils.Lingvo;

	public class LateDays : ATrace {
		public LateDays(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		public int LateDaysCount { get; private set; }
		public int LoanID { get; private set; }
		public int ScheduleID { get; private set; }
		public int TransactionID { get; private set; }
		public int Threshold { get; private set; }

		public ATrace Init(int nLateDaysCount, int nLoanID, int nScheduleID, int nTransactionID, int nThreshold) {
			LateDaysCount = nLateDaysCount;
			LoanID = nLoanID;
			ScheduleID = nScheduleID;
			TransactionID = nTransactionID;
			Threshold = nThreshold;

			if (LateDaysCount == 0)
				Comment = string.Format("customer {0} was never late.", CustomerID);
			else {
				Comment = string.Format(
					"customer {0} was {1} late in loan {2} schedule id {3} transaction id {4}.",
					CustomerID,
					Grammar.Number(LateDaysCount, "day"),
					LoanID,
					ScheduleID,
					TransactionID
				);
			} // if

			return this;
		} // Init
	} // class LateDays
} // namespace