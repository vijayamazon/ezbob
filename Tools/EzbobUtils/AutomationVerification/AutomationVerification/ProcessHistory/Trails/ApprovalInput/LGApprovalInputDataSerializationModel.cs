namespace AutomationCalculator.ProcessHistory.Trails.ApprovalInput {
	using AutomationCalculator.Common;
	using JetBrains.Annotations;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	internal class LGApprovalInputDataSerializationModel : ApprovalInputDataSerializationModel {
		[UsedImplicitly]
		[JsonConverter(typeof(StringEnumConverter))]
		public AutoDecisionFlowTypes FlowType { get; set; }

		[UsedImplicitly]
		public bool ErrorInLGData { get; set; }

		public LGApprovalInputDataSerializationModel(LGApprovalInputData src) : base(src) {
			FlowType = src.FlowType;
			ErrorInLGData = src.ErrorInLGData;
		} // constructor
	} // class LGApprovalInputDataSerializationModel
} // namespace
