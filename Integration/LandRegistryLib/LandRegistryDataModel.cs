namespace LandRegistryLib
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Xml.Serialization;

	public class LandRegistryDataModel
	{

		public string Request { get; set; }
		public LandRegistryRequestType RequestType { get; set; }
		
		public string Response { get; set; }
		public LandRegistryResponseType ResponseType { get; set; }

		public LandRegistryAttachmentModel Attachment { get; set; }
		
		public LandRegistryResModel Res { get; set; } //RES full response
		public LandRegistryEnquiryModel Enquery { get; set; } //Enquiry full response

		public LandRegistryDataSource DataSource { get; set; }
	}

	public enum LandRegistryRequestType
	{
		None = 0,
		Enquiry = 1,//EnquiryByPropertyDescription
		EnquiryPoll = 2,//EnquiryByPropertyDescriptionPoll
		Res = 3,//RegisterExtractService
		ResPoll = 4,//RegisterExtractServicePoll
	} // enum 

	public enum LandRegistryResponseType
	{
		None,
		Acknowledgement = 1,
		Rejection = 2,
		Success = 3,
		Unkown = 4
	} // enum 

	public enum LandRegistryDataSource
	{
		Cache,
		Api
	}

	public enum RestrictionTypeCode //Type of restriction present on the register.
	{
		Other = 0,
		JointProprietor = 1,//10
		ChargeRelated = 2,//20
		Charge = 3,//30
	}

	public enum RestictionScheduleCode
	{
		[Description("Schedule of Leases of Easements")]
		LeasesEasements = 10,
		[Description("Schedule of Single Registered Lease")]
		SingleRegisteredLease = 20,
		[Description("Schedule of Notice of Leases")]
		NoticeLeases = 30,
		[Description("Schedule of Registered Properties")]
		RegisteredProperties = 40,
		[Description("Schedule of Registered Rentcharges")]
		RegisteredRentcharges = 50,
		[Description("Schedule of Restrictive Covenants")]
		RestrictiveCovenants = 60,
		[Description("Schedule of Apportioned Rents")]
		ApportionedRents = 70,
		[Description("Schedule of Rentcharges")]
		Rentcharges = 80,
		[Description("Schedule of Apportionments and Exonerations")]
		ApportionmentsExonerations = 90,
		[Description("Schedule of Rentcharges created by Transfers of Part")]
		RentchargesTransfersPart = 100,
		[Description("Schedule of Personal Covenants")]
		PersonalCovenants = 110,
		[Description("Schedule of Single Registered Rentcharge Lease")]
		SingleRegisteredRentchargeLease = 120,
		[Description("Schedule of Multiple Registered Leases")]
		MultipleRegisteredLeases = 130,
		[Description("Schedule of Deeds imposing Restrictive")]
		ScheduleDeedsRestrictive = 140,
	}

	public enum RestrictionSubRegisterCode
	{
		[Description("A : Property Register")]
		A,
		[Description("B : Proprietorship Register")]
		B,
		[Description("C : Charges Register")]
		C,
		[Description("D : Cautioner's Register")]
		D,
	}

	public class LandRegistryAttachmentModel
	{
		public string FileName { get; set; }
		public string FilePath { get; set; }
		public byte[] AttachmentContent { get; set; }
	}
	public class LandRegistryAcknowledgementModel
	{
		public DateTime PollDate { get; set; }
		public string Description { get; set; }
		public string UniqueId { get; set; }
	}

	public class LandRegistryRejectionModel
	{
		public string Reason { get; set; }
		public string OtherDescription { get; set; }
	}

	public class LandRegistryEnquiryModel
	{
		public LandRegistryAcknowledgementModel Acknowledgement { get; set; } //poll response
		public LandRegistryRejectionModel Rejection { get; set; } // error response
		public List<LandRegistryEnquiryTitle> Titles { get; set; }
	}

	public class LandRegistryEnquiryTitle
	{
		public string TitleNumber { get; set; }
		public string BuildingName { get; set; }
		public string SubBuildingName { get; set; }
		public string BuildingNumber { get; set; }
		public string StreetName { get; set; }
		public string CityName { get; set; }
		public string Postcode { get; set; }
	}

	public class LandRegistryResModel
	{
		public LandRegistryAcknowledgementModel Acknowledgement { get; set; } //poll response
		public LandRegistryRejectionModel Rejection { get; set; } // error response

		public string TitleNumber { get; set; } // Note - shouldn't contain spaces for the expand\collapse functionality to work properly
		//public bool CommonholdIndicator { get; set; } //if false indicate (Indicator showing whether the title is commonhold property)
		public List<KeyValuePair<string, string>> PricePaidInfills { get; set; }
		
		public List<LandRegistryAddressModel> PropertyAddresses { get; set; }

		public List<string> Indicators { get; set; }

		public LandRegistryProprietorshipModel Proprietorship { get; set; }

		public List<LandRegistryRestrictionModel> Restrictions { get; set; }
		
		public List<LandRegistryChargeModel> Charges { get; set; }
	}

	public class LandRegistryProprietorshipModel
	{
		public DateTime? CurrentProprietorshipDate { get; set; }
		public List<ProprietorshipPartyModel> ProprietorshipParties { get; set; }
	}

	public class ProprietorshipPartyModel
	{
		public string ProprietorshipType { get; set; } //CautionerParty or RegisteredProprietorParty

		public string ProprietorshipPartyType { get; set; } //PrivateIndividual or Company
		//private
		public string PrivateIndividualForename { get; set; }
		public string PrivateIndividualSurname { get; set; }
		//company
		public string CompanyRegistrationNumber { get; set; }
		public string CompanyName { get; set; }

		public List<LandRegistryAddressModel> ProprietorshipAddresses { get; set; }
	}

	public class LandRegistryAddressModel
	{
		public string PostCode { get; set; }
		public string Lines { get; set; }
	}

	public class LandRegistryChargeModel
	{
		public DateTime ChargeDate { get; set; }
		public string Description { get; set; }
		public LandRegistryProprietorshipModel Proprietorship { get; set; }
	}

	[Serializable]
	public class LandRegistryRestrictionModel
	{
		public RestrictionTypeCode TypeCode { get; set; }
		public string Type { get; set; }
		public string EntryNumber { get; set; }
		public string EntryText { get; set; }
		public string ScheduleCode { get; set; }
		public string SubRegisterCode { get; set; }
		public List<KeyValuePair<string, string>> Infills { get; set; }
	}

	[Serializable]
	[XmlType(TypeName = "SerializableKeyValue")]
	public struct KeyValuePair<K, V>
	{
		public K Key
		{ get; set; }

		public V Value
		{ get; set; }
	}
	/*
	public class LandRegistryInfillsModel
	{
		public string Amount { get; set; }
		public string ChargeDate { get; set; }
		public string ChargeParty { get; set; }
		public string Date { get; set; }
		public string DeedDate { get; set; }
		public string DeedExtent { get; set; }
		public string DeedParty { get; set; }
		public string DeedType { get; set; }
		public string ExtentOfLand { get; set; }
		public string MiscellaneousText { get; set; }
		public string Name { get; set; }
		public string Note { get; set; }
		public string OptionalMiscText { get; set; }
		public string PlansReference { get; set; }
		public string TitleNumber { get; set; }
		public string VerbatimText { get; set; }
	}
	*/
	public static class LandRegistryIndicatorText
	{
		/// <summary>
		/// false indicators
		/// </summary>

		public const string CommonholdIndicator = "The title is not commonhold property";

		/// <summary>
		/// true indicators
		/// </summary>
		public const string AgreedNoticeIndicator = "There is an Agreed Notice in the charges register";
		public const string BankruptcyIndicator = "There is a bankruptcy entry in the register";
		public const string CautionIndicator = "There is a caution entry in the register";
		public const string CCBIIndicator = "There is a Caution/Creditor Notice/Bankruptcy/Inhibition entry in the register";
		public const string ChargeeIndicator = "There is a Chargee entry in the register";
		public const string ChargeIndicator = "There is a Charge entry in the register";
		public const string ChargeRelatedRestrictionIndicator = "There is a Charge Related Restriction entry in the register";
		public const string ChargeRestrictionIndicator = "There is a Charge Restriction entry in the register";
		public const string CreditorsNoticeIndicator = "There is a Creditors Notice entry in the register";
		public const string DeathOfProprietorIndicator = "There is an entry relating to the death of a registered proprietor of the estate";
		public const string DeedOfPostponementIndicator = "There is a Deed of Postponement entry in the register";
		public const string DiscountChargeIndicator = "There is a Discount Charge entry in the register";
		public const string EquitableChargeIndicator = "There is an Equitable Charge entry in the register";
		public const string GreenOutEntryIndicator = "There is a Green Out entry in the register";
		public const string HomeRightsChangeOfAddressIndicator = "There is a Home Rights Change Of Address entry in the register";
		public const string HomeRightsIndicator = "There is a Home Rights entry in the register";
		public const string LeaseHoldTitleIndicator = "the Title is a Leasehold Class of Title";
		public const string MultipleChargeIndicator = " multiple charges exist";
		public const string NonChargeRestrictionIndicator = "There is a Non-Charge Restriction entry in the register";
		public const string NotedChargeIndicator = "There is a Noted Charge entry in the register";
		public const string PricePaidIndicator = "There is a Price Paid entry in the register";
		public const string PropertyDescriptionNotesIndicator = " any notes exist in association with the property description.";
		public const string RentChargeIndicator = "There is a Rent Charge entry in the register";
		public const string RightOfPreEmptionIndicator = "There is a Right Of Preemption entry in the register";
		public const string ScheduleOfLeasesIndicator = "There is a Schedule Of Leases entry in the register";
		public const string SubChargeIndicator = "There is a Sub Charge entry in the register";
		public const string UnidentifiedEntryIndicator = "Indicates whether any entries exist on the register that could not be identified by the system";
		public const string UnilateralNoticeBeneficiaryIndicator = "There is a Unilateral Notice Beneficiary entry in the register";
		public const string UnilateralNoticeIndicator = "There is a Unilateral Notice in the charges register";
		public const string VendorsLienIndicator = "There is a Vendors Lien entry in the register";
	}
}
