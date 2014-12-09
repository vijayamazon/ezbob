namespace Reports.EarnedInterest {
	using System;
	using System.Globalization;

	class TransactionData {
		public readonly DateTime Date;
		public decimal Repayment;

		public TransactionData(DateTime oDate, decimal nRepayment) {
			Date = oDate;
			Repayment = nRepayment;
		} // constructor

		public override string ToString() {
			return string.Format("on {0}: {1}", Date.ToString("MMM dd yyyy", ms_oCulture), Repayment.ToString("C", ms_oCulture));
		} // ToString

		private static readonly CultureInfo ms_oCulture = new CultureInfo("en-GB", false);
	} // class TransactionData
} // namespace Reports
