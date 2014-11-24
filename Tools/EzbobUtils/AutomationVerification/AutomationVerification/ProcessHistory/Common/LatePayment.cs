namespace AutomationCalculator.ProcessHistory.Common {
	using System;
	using System.Globalization;
	using Ezbob.Utils.Lingvo;

	public class LatePayment : ATrace {
		#region constructor

		public LatePayment(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {
		} // constructor

		#endregion constructor

		#region method Init
		public void Init(int delay, int threshhold) {
			if (DecisionStatus == DecisionStatus.Affirmative)
			{
				Comment = string.Format(
					"customer has no payments that are more than {0} late",
					Grammar.Number(Threshold, "day")
				);
			}
			else
			{
				Comment = string.Format(
					"customer was {0} late; allowed delay: {1}",
					Grammar.Number(delay, "day"),
					Grammar.Number(Threshold, "day")
				);
			} // if
		}


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
					"there are no payments that are more than {0} late",
					Grammar.Number(Threshold, "day")
				);
			}
			else {
				Comment = string.Format(
					"customer was {1} late for loan {2}, schedule id {3}, transaction id {4}; " +
					"paid on {5} instead of {6}; allowed delay: {0}",
					Grammar.Number(Threshold, "day"),
					Grammar.Number(Delay, "day"),
					LoanID,
					ScheduleID,
					TransactionID,
					TransactionTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
					ScheduleDate.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture)
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

		public int Delay
		{
			get { return (int)(TransactionTime.Date - ScheduleDate.Date).TotalDays; } // get
		} // Delay
	} // class LatePayment
} // namespace
