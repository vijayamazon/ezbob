namespace AutomationCalculator.Turnover {
	public class SimpleOneMonthValue : AOneMonthValue {
		public SimpleOneMonthValue() {
			amount = 0;
		} // constructor

		public SimpleOneMonthValue(TurnoverDbRow row) : this() {
			Add(row);
		} // constructor

		public override decimal Amount { get {return this.amount; } }

		public override void Add(TurnoverDbRow row) {
			amount += row.Turnover;
		} // Add

		private decimal amount;
	} // class SimpleOneMonthValue
} // namespace
