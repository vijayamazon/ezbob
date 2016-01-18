namespace AutomationCalculator.ProcessHistory.AutoRejection {
	public class LGHardReject : ABoolTrace {
		public LGHardReject(DecisionStatus status) : base(status) {
		} // constructor

		protected override string PropertyName {
			get { return "Logical Glue hard rejection"; }
		} // PropertyName
	} // class LGHardReject
} // namespace
