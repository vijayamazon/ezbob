namespace AutomationCalculator.ProcessHistory.Common {
	public class HasBucket : ABoolTrace {
		public HasBucket(DecisionStatus status) : base(status) {
		} // constructor

		protected override string PropertyName {
			get { return "bucket"; }
		} // PropertyName
	} // class HasBucket
} // namespace
