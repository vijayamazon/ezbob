namespace Reports.EarnedInterest {
	using System;
	using System.Globalization;

	class InterestData {
		public readonly DateTime Date;
		public int PeriodLength;
		public decimal OriginalInterest;

		public InterestData(DateTime oDate, decimal nOriginalInterest) {
			PeriodLength = 0;
			Date = oDate;
			OriginalInterest = nOriginalInterest;
		} // constructor

		public decimal Interest {
			get { return OriginalInterest / DaysInMonth; }
		} // Interest

		private int DaysInMonth {
			get {
				DateTime d = Date.AddMonths(-1);
				return DateTime.DaysInMonth(d.Year, d.Month);
			} // get
		} // DaysInMonth

		public override string ToString() {
			return string.Format(
				"on {0}: {1} = {2} / {3} - for {4} days",
				Date.ToString("MMM dd yyyy", ms_oCulture),
				Interest.ToString(ms_oCulture).PadLeft(30, ' '),
				OriginalInterest.ToString(ms_oCulture).PadLeft(30, ' '),
				DaysInMonth.ToString("N0", ms_oCulture),
				PeriodLength.ToString("N0", ms_oCulture)
			);
		} // ToString

		private static readonly CultureInfo ms_oCulture = new CultureInfo("en-GB", false);
	} // class InterestData
} // namespace
