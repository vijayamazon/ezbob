namespace DbConstants {
	using System.ComponentModel;
	using System.Runtime.Serialization;

	[DataContract]
	public enum PricingCalcuatorScenarioNames {
		[EnumMember]
		[Description("Basic New")]
		BasicNew,

		[EnumMember]
		[Description("Basic Repeating")]
		BasicRepeating,

		[EnumMember]
		[Description("Broker")]
		Broker,

		[EnumMember]
		[Description("Non-ltd")]
		NonLimited,

		[EnumMember]
		[Description("Small Loan")]
		SmallLoan,

		[EnumMember]
		[Description("Sole Traders")]
		SoleTraders,
	} // enum PricingCalcuatorScenarioNames
} // namespace
