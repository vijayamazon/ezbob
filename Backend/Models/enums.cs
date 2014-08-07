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

	#region enum EmailConfirmationRequestState

	[DataContract]
	public enum EmailConfirmationRequestState {
		/// <summary>
		/// The state of the email is unknown. For old customers.
		/// </summary>
		[EnumMember]
		Unknown = 0,
		/// <summary>
		/// Request was issued, but not confirmed.
		/// </summary>
		[EnumMember]
		Pending = 1,
		/// <summary>
		/// The email in this request was confirmed.
		/// </summary>
		[EnumMember]
		Confirmed = 2,
		/// <summary>
		/// The request is cancelled and not valid any more.
		/// </summary>
		[EnumMember]
		Canceled = 3,
		/// <summary>
		/// Manually confirmed customer.
		/// </summary>
		[EnumMember]
		ManuallyConfirmed = 4,
		/// <summary>
		/// The request is confirmed because other request for this customer has been confirmed.
		/// </summary>
		[EnumMember]
		ImplicitlyConfirmed = 5,

		[EnumMember]
		_MAX_
	} // enum EmailConfirmationRequestState

	#endregion enum EmailConfirmationRequestState

	#region enum EmailConfirmationResponse

	[DataContract]
	public enum EmailConfirmationResponse {
		/// <summary>
		/// This member must be the first.
		/// </summary>
		[EnumMember]
		NotDone = 0,

		/// <summary>
		/// This member must be the second.
		/// </summary>
		[EnumMember]
		Confirmed = 1,

		[EnumMember]
		NotFound = 2,

		[EnumMember]
		InvalidState = 3,

		/// <summary>
		/// This member must be the last.
		/// </summary>
		[EnumMember]
		OtherError
	} // enum EmailConfirmationResponse

	#endregion enum EmailConfirmationResponse
} // namespace
