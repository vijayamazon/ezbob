namespace Ezbob.Backend.Strategies.CalculateLoan.Helpers {
	using System;
	using System.Globalization;

	internal class OneDayLoanStatus {
		public OneDayLoanStatus(DateTime d, decimal openPrincipal) {
			Date = d;
			OpenPrincipal = openPrincipal;
			AssignedFees = 0;
			DailyInterestRate = 0;
		} // constructor

		public DateTime Date {
			get { return this.date; }
			set { this.date = value.Date; }
		} // Date

		public decimal OpenPrincipal { get; set; }
		public decimal DailyInterest {
			get { return OpenPrincipal * DailyInterestRate; }
		} // DailyInterest
		public decimal AssignedFees { get; set; }

		public decimal DailyInterestRate { get; set; }

		public decimal RepaidPrincipal { get; set; }
		public decimal RepaidInterest { get; set; }
		public decimal RepaidFees { get; set; }

		public void AddRepayment(Repayment rp) {
			if (rp == null)
				return;

			RepaidPrincipal += rp.Principal;
			RepaidInterest += rp.Interest;
			RepaidFees += rp.Fees;
		} // AddRepayment

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"on {0}: p{1} i{2} f{3} (dr {4}) rp{5} ri{6} rf{7}",
				Date.DateStr(),
				OpenPrincipal.ToString("C2", Culture),
				DailyInterest.ToString("C2", Culture),
				AssignedFees.ToString("C2", Culture),
				DailyInterestRate.ToString("P4", Culture),
				RepaidPrincipal.ToString("C2", Culture),
				RepaidInterest.ToString("C2", Culture),
				RepaidFees.ToString("C2", Culture)
			);
		} // ToString

		private static CultureInfo Culture {
			get { return Library.Instance.Culture; }
		} // Culture

		private DateTime date;
	} // class OneDayLoanStatus
} // namespace
