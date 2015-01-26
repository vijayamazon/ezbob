namespace AutomationCalculator.Turnover {
	using System;

	public class OnlineOneMonthValue : AOneMonthValue {
		public OnlineOneMonthValue() {
			this.ebay = 0;
			this.other = 0;
			this.payPal = 0;
		} // constructor

		public OnlineOneMonthValue(TurnoverDbRow row) : this() {
			Add(row);
		} // constructor

		public override decimal Amount {
			get { return Math.Max(this.ebay, this.payPal) + this.other; } // get
		} // Value

		public override void Add(TurnoverDbRow r) {
			if (r.MpTypeID == MpType.Ebay)
				this.ebay += r.Turnover;
			else if (r.MpTypeID == MpType.PayPal)
				this.payPal += r.Turnover;
			else
				this.other += r.Turnover;
		} // Add

		private decimal ebay;
		private decimal payPal;
		private decimal other;
	} // class OnlineOneMonthValue
} // namespace
