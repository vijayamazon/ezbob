namespace AutomationCalculator.ProcessHistory.Common {
	public class LGDataFound : ABoolTrace {
		public LGDataFound(DecisionStatus status) : base(status) {
		} // constructor

		protected override string PropertyName {
			get { return "Logical Glue data"; }
		} // PropertyName
	} // class LGDataFound
} // namespace
