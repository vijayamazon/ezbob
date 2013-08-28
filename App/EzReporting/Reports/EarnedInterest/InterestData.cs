using System;

namespace Reports {
	#region class InterestData

	class InterestData {
		#region public

		public readonly DateTime Date;
		public int PeriodLength;
		public decimal OriginalInterest;

		#region constructor

		public InterestData(DateTime oDate, decimal nOriginalInterest) {
			PeriodLength = 0;
			Date = oDate;
			OriginalInterest = nOriginalInterest;
		} // constructor

		#endregion constructor

		#region property Interest

		public decimal Interest {
			get { return OriginalInterest / DaysInMonth; }
		} // Interest

		#endregion property Interest

		#region property DaysInMonth

		private int DaysInMonth {
			get {
				DateTime d = Date.AddMonths(-1);
				return DateTime.DaysInMonth(d.Year, d.Month);
			} // get
		} // DaysInMonth
		#endregion property DaysInMonth

		#region method ToStirng

		public override string ToString() {
			return string.Format("on {0}: {1} = {2} / {3} - for {4} days", Date, Interest, OriginalInterest, DaysInMonth, PeriodLength);
		} // ToString

		#endregion method ToStirng

		#endregion public
	} // class InterestData

	#endregion class InterestData
} // namespace Reports
