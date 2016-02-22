namespace AutomationCalculator.ProcessHistory.Common {
	public class InternalFlow : ATrace {
		public InternalFlow(DecisionStatus status) : base(status) {
		} // constructor

		public void Init() {
			Comment = "Internal flow was followed.";
		} // Init
	} // class InternalFlow
} // namespace
