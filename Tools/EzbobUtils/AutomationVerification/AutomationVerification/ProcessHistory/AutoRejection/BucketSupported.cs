namespace AutomationCalculator.ProcessHistory.AutoRejection {
	public class BucketSupported : ABoolTrace {
		public BucketSupported(DecisionStatus status) : base(status) {
		} // constructor

		protected override string PropertyName {
			get { return "bucket supported by customer's brand"; }
		} // PropertyName
	} // class BucketSupported
} // namespace
