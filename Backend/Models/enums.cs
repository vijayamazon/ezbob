namespace Ezbob.Backend.Models {
	using System.Runtime.Serialization;

	#region enum NewCreditLineOption

	[DataContract]
	public enum NewCreditLineOption {
		[EnumMember]
		SkipEverything = 1,

		[EnumMember]
		UpdateEverythingExceptMp = 2,

		[EnumMember]
		UpdateEverythingAndApplyAutoRules = 3,

		[EnumMember]
		UpdateEverythingAndGoToManualDecision = 4,
	} // enum NewCreditLineOption

	#endregion enum NewCreditLineOption

	#region enum FraudMode

	[DataContract]
	public enum FraudMode {
		[EnumMember]
		FullCheck,

		[EnumMember]
		PersonalDetaisCheck,

		[EnumMember]
		CompanyDetailsCheck,

		[EnumMember]
		MarketplacesCheck,
	} // enum FraudMode

	#endregion enum FraudMode
} // namespace Ezbob.Backend.Models
