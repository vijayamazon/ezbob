namespace Reports.EarnedInterest {
	using System;
	using System.Globalization;

	class TransactionData {
		public readonly DateTime Date;
		public decimal Repayment;

		#region constructor

		public TransactionData(DateTime oDate, decimal nRepayment) {
			Date = oDate;
			Repayment = nRepayment;
		} // constructor

		#endregion constructor

		#region method ToStirng

		public override string ToString() {
			return string.Format("on {0}: {1}", Date.ToString("MMM dd yyyy", ms_oCulture), Repayment.ToString("C", ms_oCulture));
		} // ToString

		#endregion method ToStirng

		private static readonly CultureInfo ms_oCulture = new CultureInfo("en-GB", false);
	} // class TransactionData
} // namespace Reports
