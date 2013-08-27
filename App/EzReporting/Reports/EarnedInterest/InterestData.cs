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
			get { return PeriodLength == 0 ? OriginalInterest : OriginalInterest / PeriodLength; }
		} // Interest

		#endregion property Interest

		#region method ToStirng

		public override string ToString() {
			return string.Format("on {0}: {1} = {2} / {3}", Date, Interest, OriginalInterest, PeriodLength);
		} // ToString

		#endregion method ToStirng

		#endregion public
	} // class InterestData

	#endregion class InterestData
} // namespace Reports
