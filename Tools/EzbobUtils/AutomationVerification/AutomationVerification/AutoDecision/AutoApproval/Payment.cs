namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Globalization;
	using AutomationCalculator.ProcessHistory.Common;
	using Newtonsoft.Json;

	public static class PaymentExt {
		public static string Stringify(this Payment p) {
			return string.Format(
				"loan {0}, sch id {1} on {2}, trn id {3} on {4}",
				p.LoanID,
				p.ScheduleID,
				p.ScheduleDate.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture),
				p.TransactionID,
				p.TransactionTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
				);
		} // Stringify
	} // class PaymentExt

	/// <summary>
	///     Customer late payment.
	///     One of the outputs of LoadAutoApprovalData sp.
	/// </summary>
	public class Payment {
		public int LoanID { get; set; }
		public int ScheduleID { get; set; }
		public DateTime ScheduleDate { get; set; }
		public int TransactionID { get; set; }
		public DateTime TransactionTime { get; set; }

		[JsonIgnore]
		public int Delay {
			get { return (int)(TransactionTime.Date - ScheduleDate.Date).TotalDays; } // get
		} // Delay

		public bool IsLate(int nDays) {
			return Delay > nDays;
		} // IsLate

		public void Fill(LatePayment lp, int nThreshold) {
			lp.Init(LoanID, ScheduleID, ScheduleDate, TransactionID, TransactionTime, nThreshold);
		} // Fill
	} // class Payment
} // namespace
