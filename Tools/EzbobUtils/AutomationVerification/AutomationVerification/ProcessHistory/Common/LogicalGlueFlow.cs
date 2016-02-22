namespace AutomationCalculator.ProcessHistory.Common {
	public class LogicalGlueFlow : ATrace {
		public LogicalGlueFlow(DecisionStatus status) : base(status) {
		} // constructor

		public void Init() {
			Comment = "Logical Glue flow was followed.";
		} // Init
	} // class LogicalGlueFlow
} // namespace
