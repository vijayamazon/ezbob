namespace AutomationCalculator.ProcessHistory.Trails.ApprovalInput {
	using AutomationCalculator.Common;
	using Newtonsoft.Json;

	public class LGApprovalInputData : ApprovalInputData {
		public LGApprovalInputData() {
			FlowType = AutoDecisionFlowTypes.Unknown;
			ErrorInLGData = true;
		} // constructor

		public void FullInit(AutoDecisionFlowTypes flowType, bool errorInLGData, ApprovalInputData aid) {
			base.FullInit(aid);
			FlowType = flowType;
			ErrorInLGData = errorInLGData;
		} // FullInit

		public AutoDecisionFlowTypes FlowType { get; set; }

		public bool ErrorInLGData { get; set; }

		public override string Serialize() {
			return JsonConvert.SerializeObject(new LGApprovalInputDataSerializationModel(this), Formatting.Indented);
		} // Serialize
	} // class LGApprovalInputData
} // namespace
