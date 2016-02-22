namespace AutomationCalculator.ProcessHistory.Common {
	public class LGWithoutError : ABoolTrace {
		public LGWithoutError(DecisionStatus status) : base(status) {
		} // constructor

		protected override string PropertyName {
			get { return "Logical Glue data without error"; }
		} // PropertyName
	} // class LGWithoutError
} // namespace
