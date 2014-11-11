namespace AutomationCalculator.ProcessHistory.Common {
	using System;
	using System.Globalization;
	using Ezbob.Utils.Lingvo;

	public class LatePayment : ATrace {
		#region constructor

		public LatePayment(int nCustomerID, bool bCompletedSuccessfully) : base(nCustomerID, bCompletedSuccessfully) {
		} // constructor

		#endregion constructor

		#region method Init

		public void Init(
			int nLoanID,
			int nScheduleID,
			DateTime oScheduleDate,
			int nTransactionID,
			DateTime oTransactionTime,
			int nThreshold
		) {
			LoanID = nLoanID;
			ScheduleID = nScheduleID;
			ScheduleDate = oScheduleDate;
			TransactionID = nTransactionID;
			TransactionTime = oTransactionTime;

			Threshold = nThreshold;

			if (LoanID == 0) {
				Comment = string.Format(
					"customer {0} has no payments that are more than {1} late",
					CustomerID,
					Grammar.Number(Threshold, "day")
				);
			}
			else {
				Comment = string.Format(
					"customer {0} was {1} late for loan {2}, schedule id {3}, transaction id {4}; " +
					"paid on {5} instead of {6}; allowed delay: {7}",
					CustomerID,
					Grammar.Number(Delay, "day"),
					LoanID,
					ScheduleID,
					TransactionID,
					TransactionTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
					ScheduleDate.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture),
					Grammar.Number(Threshold, "day")
				);
			} // if
		} // Init

		#endregion method Init

		public int LoanID { get; private set; }
		public int ScheduleID { get; private set; }
		public DateTime ScheduleDate { get; private set; }
		public int TransactionID { get; private set; }
		public DateTime TransactionTime { get; private set; }

		public int Threshold { get; private set; }

		public int Delay {
			get { return (int)(TransactionTime - ScheduleDate).TotalDays; } // get
		} // Delay
	} // class LatePayment
} // namespace
