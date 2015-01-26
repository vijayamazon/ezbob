namespace AutomationCalculator.Turnover {
	public abstract class AOneMonthValue {
		public abstract decimal Amount { get; }
		public abstract void Add(TurnoverDbRow row);
	} // class AOneMonthValue
} // namespace
