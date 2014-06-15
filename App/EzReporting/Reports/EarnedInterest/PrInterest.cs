namespace Reports.EarnedInterest {
	using System;
	using System.Globalization;

	class PrInterest {
		public readonly DateTime Date;
		public decimal Principal;
		public decimal Interest;

		public PrInterest(DateTime oDate, decimal nPrincipal) {
			Date = oDate;
			Principal = nPrincipal;
			Interest = 0;
		} // constructor

		public override string ToString() {
			return string.Format(
				"{0}: {1} * {2} = {3}",
				Date.ToString("MMM dd yyyy", ms_oCulture),
				Principal.ToString("C2", ms_oCulture).PadLeft(20, ' '),
				Interest.ToString(ms_oCulture).PadLeft(30, ' '),
				(Principal * Interest).ToString("C2", ms_oCulture)
			);
		} // ToString

		public bool Update(InterestData oDelta, InterestFreezePeriods ifp, BadPeriods bp) {
			if (bp == null) {
				if (ifp == null)
					Interest = oDelta.Interest;
				else
					Interest = ifp.GetInterest(Date) ?? oDelta.Interest;
			}
			else
				Interest = bp.Contains(Date) ? 0 : oDelta.Interest;

			return Date == oDelta.Date;
		} // Update

		public void Update(TransactionData oDelta) {
			if (Date > oDelta.Date)
				Principal -= oDelta.Repayment;
		} // Update

		private static readonly CultureInfo ms_oCulture = new CultureInfo("en-GB", false);
	} // class PrInterest
} // namespace Reports
