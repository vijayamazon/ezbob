namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using System.Xml;
	using Logger;

	[DataContract]
	[DLB5]
	public class ExperianLtdDLB5 : AExperianLtdDataRow {
		public ExperianLtdDLB5(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) { } // constructor

		[DataMember]
		[DLB5("RECORDTYPE")]
		public string RecordType { get; set; }
		[DataMember]
		[DLB5("ISSUINGCOMPANY")]
		public string IssueCompany { get; set; }
		[DataMember]
		[DLB5("CURPREVFLAG")]
		public string CurrentpreviousIndicator { get; set; }
		[DataMember]
		[DLB5("EFFECTIVEDATE-YYYY")]
		public DateTime? EffectiveDate { get; set; }
		[DataMember]
		[DLB5("SHARECLASSNUM")]
		public string ShareClassNumber { get; set; }
		[DataMember]
		[DLB5("SHAREHOLDINGNUM")]
		public string ShareholdingNumber { get; set; }
		[DataMember]
		[DLB5("SHAREHOLDERNUM")]
		public string ShareholderNumber { get; set; }
		[DataMember]
		[DLB5("SHAREHOLDERTYPE")]
		public string ShareholderType { get; set; }
		[DataMember]
		[DLB5("NAMEPREFIX")]
		public string Prefix { get; set; }
		[DataMember]
		[DLB5("FIRSTNAME")]
		public string FirstName { get; set; }
		[DataMember]
		[DLB5("MIDNAME")]
		public string MidName1 { get; set; }
		[DataMember]
		[DLB5("SURNAME")]
		public string LastName { get; set; }
		[DataMember]
		[DLB5("NAMESUFFIX")]
		public string Suffix { get; set; }
		[DataMember]
		[DLB5("QUAL")]
		public string ShareholderQualifications { get; set; }
		[DataMember]
		[DLB5("TITLE")]
		public string Title { get; set; }
		[DataMember]
		[DLB5("COMPANYNAME")]
		public string ShareholderCompanyName { get; set; }
		[DataMember]
		[DLB5("SHAREHOLDERKGEN")]
		public string KgenName { get; set; }
		[DataMember]
		[DLB5("SHAREHOLDERREGNUM")]
		public string ShareholderRegisteredNumber { get; set; }
		[DataMember]
		[DLB5("ADDRLINE1")]
		public string AddressLine1 { get; set; }
		[DataMember]
		[DLB5("ADDRLINE2")]
		public string AddressLine2 { get; set; }
		[DataMember]
		[DLB5("ADDRLINE3")]
		public string AddressLine3 { get; set; }
		[DataMember]
		[DLB5("TOWN")]
		public string Town { get; set; }
		[DataMember]
		[DLB5("COUNTY")]
		public string County { get; set; }
		[DataMember]
		[DLB5("POSTCODE")]
		public string Postcode { get; set; }
		[DataMember]
		[DLB5("COUNTRYOFORIGIN")]
		public string Country { get; set; }
		[DataMember]
		[DLB5("PUNAPOSTCODE")]
		public string ShareholderPunaPcode { get; set; }
		[DataMember]
		[DLB5("RMC")]
		public string ShareholderRMC { get; set; }
		[DataMember]
		[DLB5("SUPPRESS")]
		public string SuppressionFlag { get; set; }
		[DataMember]
		[DLB5("NOCREF")]
		public string NOCRefNumber { get; set; }
		[DataMember]
		[DLB5("LASTUPDATEDDATE-YYYY")]
		public DateTime? LastUpdated { get; set; }
	} // class ExperianLtdDLB5
} // namespace
