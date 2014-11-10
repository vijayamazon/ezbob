namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.ReApproval {
	using System;
	using AutomationCalculator.ProcessHistory.ReApproval;

	internal class Payment {
		public int LoanID { get; set; }
		public int ScheduleID { get; set;} 
		public DateTime ScheduleDate { get; set;} 
		public int TransactionID { get; set;} 
		public DateTime TransactionTime { get; set;}

		public int Delay {
			get { return (int)(TransactionTime - ScheduleDate).TotalDays; } // get
		} // Delay

		public bool IsLate(int nDays) {
			return Delay > nDays;
		} // IsLate

		public void Fill(LatePayment lp, int nThreshold) {
			lp.Init(LoanID, ScheduleID, ScheduleDate, TransactionID, TransactionTime, nThreshold);
		} // Fill
	} // class Payment
} // namespace
