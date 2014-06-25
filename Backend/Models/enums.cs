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

	#region enum ConfigTableType

	[DataContract]
	public enum ConfigTableType {
		[EnumMember]
		LoanOfferMultiplier,

		[EnumMember]
		BasicInterestRate,

		[EnumMember]
		EuLoanMonthlyInterest,

		[EnumMember]
		DefaultRateCompany,

		[EnumMember]
		DefaultRateCustomer,
	} // enum ConfigTableType

	#endregion enum ConfigTableType

	#region enum VatReturnSourceType

	public enum VatReturnSourceType {
		Linked = 1,
		Uploaded = 2,
		Manual = 3,
	} // enum VatReturnSourceType

	#endregion enum VatReturnSourceType

	#region enum AffordabilityType

	[DataContract]
	public enum AffordabilityType {
		[EnumMember]
		Hmrc,

		[EnumMember]
		Bank,

		[EnumMember]
		Psp,

		[EnumMember]
		Ecomm, 

		[EnumMember]
		Accounting,
	} // AffordabilityType

	#endregion enum AffordabilityType
} // namespace Ezbob.Backend.Models
