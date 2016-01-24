namespace AutomationCalculator.Common {
	public class AutoRejectionOutput {
		public AutoRejectionOutput() {
			FlowType = AutoDecisionFlowTypes.Unknown;
			ProductSubTypeID = 0;
			GradeRangeID = 0;
			ErrorInLGData = true;
		} // constructor

		public AutoDecisionFlowTypes FlowType { get; set; }

		public int ProductSubTypeID { get; set; }

		public int GradeRangeID { get; set; }

		public bool ErrorInLGData { get; set; }
	} // class AutoRejectionOutput
} // namespace
