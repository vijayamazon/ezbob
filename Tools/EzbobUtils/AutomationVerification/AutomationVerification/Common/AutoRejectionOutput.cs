namespace AutomationCalculator.Common {
	public class AutoRejectionOutput {
		public AutoRejectionOutput() {
			FlowType = AutoDecisionFlowTypes.Unknown;
			GradeRangeID = 0;
		} // constructor

		public AutoDecisionFlowTypes FlowType { get; set; }

		public int GradeRangeID { get; set; }
	} // class AutoRejectionOutput
} // namespace
