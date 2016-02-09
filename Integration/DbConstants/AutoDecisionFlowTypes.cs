namespace AutomationCalculator.Common {
	using System.Runtime.Serialization;

	[DataContract]
	public enum AutoDecisionFlowTypes {
		[EnumMember]
		Unknown = 0,

		[EnumMember]
		Internal = 1,

		[EnumMember]
		LogicalGlue = 2,
	} // class AutoDecisionFlowTypes
} // namespace
