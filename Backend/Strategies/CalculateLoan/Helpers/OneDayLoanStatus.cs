namespace Ezbob.Backend.Strategies.CalculateLoan.Helpers {
	using System;
	using System.Globalization;
	using Ezbob.Backend.Strategies.Extensions;

	internal class OneDayLoanStatus {
		public static string FormatField(string field, int length) {
			string preResult = " " + (field ?? string.Empty) + " ";
			return (length >= 0) ? preResult.PadLeft(length + 2) : preResult.PadRight(2 - length);
		} // FormatField

		public OneDayLoanStatus(DateTime d, decimal openPrincipal) {
			Date = d;
			OpenPrincipal = openPrincipal;
			AssignedFees = 0;
			DailyInterestRate = 0;

			Str = new FormattedData(this);
		} // constructor

		public DateTime Date {
			get { return this.date.Date; }
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
				Str.Date,
				Str.OpenPrincipal,
				Str.DailyInterest,
				Str.AssignedFees,
				Str.DailyInterestRate,
				Str.RepaidPrincipal,
				Str.RepaidInterest,
				Str.RepaidFees
			);
		} // ToString

		public string ToFormattedString(
			string rowPrefix,
			string separator,
			string note,
			int dateLen,
			int openPrincipalLen,
			int dailyInterestLen,
			int assignedFeesLen,
			int dailyInterestRateLen,
			int repaidPrincipalLen,
			int repaidInterestLen,
			int repaidFeesLen,
			int notesLen
		) {
			return string.Format(
				"{0}{1}{2}{1}",
				rowPrefix,
				separator,
				string.Join(separator,
					FormatField(Str.Date, dateLen),
					FormatField(Str.OpenPrincipal, openPrincipalLen),
					FormatField(Str.DailyInterest, dailyInterestLen),
					FormatField(Str.AssignedFees, assignedFeesLen),
					FormatField(Str.DailyInterestRate, dailyInterestRateLen),
					FormatField(Str.RepaidPrincipal, repaidPrincipalLen),
					FormatField(Str.RepaidInterest, repaidInterestLen),
					FormatField(Str.RepaidFees, repaidFeesLen),
					FormatField(note, notesLen)
				)
			);
		} // ToFormattedString

		public class FormattedData {
			internal FormattedData(OneDayLoanStatus odls) { this.odls = odls; }

			public string Date { get { return this.odls.Date.DateStr(); } }
			public string OpenPrincipal { get { return this.odls.OpenPrincipal.ToString("C8", Culture); } }
			public string DailyInterest { get { return this.odls.DailyInterest.ToString("C8", Culture); } }
			public string AssignedFees { get { return this.odls.AssignedFees.ToString("C8", Culture); } }
			public string DailyInterestRate { get { return this.odls.DailyInterestRate.ToString("P8", Culture); } }
			public string RepaidPrincipal { get { return this.odls.RepaidPrincipal.ToString("C8", Culture); } }
			public string RepaidInterest { get { return this.odls.RepaidInterest.ToString("C8", Culture); } }
			public string RepaidFees { get { return this.odls.RepaidFees.ToString("C8", Culture); } }

			private readonly OneDayLoanStatus odls;
		} // class FormattedData

		public FormattedData Str { get; private set; }

		private static CultureInfo Culture {
			get { return Library.Instance.Culture; }
		} // Culture

		private DateTime date;
	} // class OneDayLoanStatus
} // namespace
