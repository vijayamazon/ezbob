namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;

	internal class CurrentLoanStatus {
		public CurrentLoanStatus(DateTime d, decimal openPrincipal) {
			Date = d;
			OpenPrincipal = openPrincipal;
			DailyInterest = 0;
			AssignedFees = 0;
		} // constructor

		public DateTime Date { get; set; }
		public decimal OpenPrincipal { get; set; }
		public decimal DailyInterest { get; set; }
		public decimal AssignedFees { get; set; }
	} // class CurrentLoanStatus
} // namespace
