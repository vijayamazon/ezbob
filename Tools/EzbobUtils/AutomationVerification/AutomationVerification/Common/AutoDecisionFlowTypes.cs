namespace AutomationCalculator.Common {
	using System.Runtime.Serialization;

	[DataContract]
	public enum AutoDecisionFlowTypes {
		[EnumMember]
		Unknown,

		[EnumMember]
		LogicalGlue,

		[EnumMember]
		Internal,
	} // class AutoDecisionFlowTypes
} // namespace
