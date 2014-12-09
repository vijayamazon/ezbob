namespace Ezbob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval {
	using System;
	using System.Globalization;
	using AutomationCalculator.ProcessHistory.Common;

	public class Payment {
		public int LoanID { get; set; }
		public int ScheduleID { get; set;} 
		public DateTime ScheduleDate { get; set;} 
		public int TransactionID { get; set;} 
		public DateTime TransactionTime { get; set;}

		public int Delay {
			get { return (int)(TransactionTime.Date - ScheduleDate.Date).TotalDays; } // get
		} // Delay

		public bool IsLate(int nDays) {
			return Delay > nDays;
		} // IsLate

		public void Fill(LatePayment lp, int nThreshold) {
			lp.Init(LoanID, ScheduleID, ScheduleDate, TransactionID, TransactionTime, nThreshold);
		} // Fill

		public override string ToString() {
			return string.Format(
				"LoanID: {0}, Schedule: {1} at {2}, Transaction: {3} at {4}, Delay: {5}",
				LoanID,
				ScheduleID,
				ScheduleDate.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				TransactionID,
				TransactionTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				Delay
			);
		} // ToString
	} // class Payment
} // namespace
