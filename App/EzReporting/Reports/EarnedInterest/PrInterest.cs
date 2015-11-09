namespace Reports.EarnedInterest {
	using System;
	using System.Globalization;

	internal class PrInterest {
		public readonly DateTime Date;
		public decimal Principal;
		public decimal Interest;

		public PrInterest(DateTime oDate, decimal nPrincipal) {
			this.Date = oDate;
			this.Principal = nPrincipal;
			this.Interest = 0;
		} // constructor

		public override string ToString() {
			return string.Format(
				"{0}: {1} * {2} = {3}",
				this.Date.ToString("MMM dd yyyy", culture),
				this.Principal.ToString("C2", culture).PadLeft(20, ' '),
				this.Interest.ToString(culture).PadLeft(30, ' '),
				(this.Principal * this.Interest).ToString("C2", culture)
			);
		} // ToString

		public void Update(
			InterestData oDelta,
			InterestFreezePeriods ifp,
			BadPeriods bp,
			bool bAccountingMode,
			DateTime? oWriteOffDate
		) {
			if (bAccountingMode) {
				if (oWriteOffDate.HasValue)
					this.Interest = (this.Date < oWriteOffDate.Value) ? GetIfpInterest(ifp, oDelta) : 0;
				else
					this.Interest = GetIfpInterest(ifp, oDelta);
			}
			else { // i.e. normal mode
				if (bp == null)
					this.Interest = GetIfpInterest(ifp, oDelta);
				else
					this.Interest = bp.Contains(this.Date) ? 0 : GetIfpInterest(ifp, oDelta);
			} // if
		} // Update

		public void Update(TransactionData oDelta) {
			if (this.Date > oDelta.Date)
				this.Principal -= oDelta.Repayment;
		} // Update

		private decimal GetIfpInterest(InterestFreezePeriods ifp, InterestData oDelta) {
			if (ifp == null)
				return oDelta.Interest;

			return ifp.GetInterest(this.Date) ?? oDelta.Interest;
		} // GetIfpInterest

		private static readonly CultureInfo culture = new CultureInfo("en-GB", false);
	} // class PrInterest
} // namespace Reports
