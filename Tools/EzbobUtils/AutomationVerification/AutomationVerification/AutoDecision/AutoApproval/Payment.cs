namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using AutomationCalculator.ProcessHistory.Common;
	using Newtonsoft.Json;

	public class Payment {
		public int LoanID { get; set; }
		public int ScheduleID { get; set;} 
		public DateTime ScheduleDate { get; set;} 
		public int TransactionID { get; set;} 
		public DateTime TransactionTime { get; set;}

		[JsonIgnore]
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
