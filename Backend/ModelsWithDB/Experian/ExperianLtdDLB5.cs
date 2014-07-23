namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Attributes;
	using Logger;

	[DataContract]
	[DLB5]
	public class ExperianLtdDLB5 : AExperianLtdDataRow {
		public ExperianLtdDLB5(ASafeLog oLog = null) : base(oLog) { } // constructor

		[DataMember]
		[DLB5("RECORDTYPE", "Record type")]
		public string RecordType { get; set; }
		[DataMember]
		[DLB5("ISSUINGCOMPANY", "Issue company")]
		public string IssueCompany { get; set; }
		[DataMember]
		[DLB5("CURPREVFLAG", "Current/previous indicator")]
		public string CurrentpreviousIndicator { get; set; }
		[DataMember]
		[DLB5("EFFECTIVEDATE-YYYY", "Effective Date")]
		public DateTime? EffectiveDate { get; set; }
		[DataMember]
		[DLB5("SHARECLASSNUM", "Share class number")]
		public string ShareClassNumber { get; set; }
		[DataMember]
		[DLB5("SHAREHOLDINGNUM", "Shareholding number")]
		public string ShareholdingNumber { get; set; }
		[DataMember]
		[DLB5("SHAREHOLDERNUM", "Shareholder number")]
		public string ShareholderNumber { get; set; }
		[DataMember]
		[DLB5("SHAREHOLDERTYPE", "Shareholder type")]
		public string ShareholderType { get; set; }
		[DataMember]
		[DLB5("NAMEPREFIX", "Shareholder Name")]
		public string Prefix { get; set; }
		[DataMember]
		[DLB5("FIRSTNAME", "Shareholder Name", DisplayPrefix = " ")]
		public string FirstName { get; set; }
		[DataMember]
		[DLB5("MIDNAME", "Shareholder Name", DisplayPrefix = " ")]
		public string MidName1 { get; set; }
		[DataMember]
		[DLB5("SURNAME", "Shareholder Name", DisplayPrefix = " ")]
		public string LastName { get; set; }
		[DataMember]
		[DLB5("NAMESUFFIX", "Shareholder Name", DisplayPrefix = " ")]
		public string Suffix { get; set; }
		[DataMember]
		[DLB5("QUAL", "Shareholder qualifications")]
		public string ShareholderQualifications { get; set; }
		[DataMember]
		[DLB5("TITLE", "Shareholder title")]
		public string Title { get; set; }
		[DataMember]
		[DLB5("COMPANYNAME", "Shareholder company name")]
		public string ShareholderCompanyName { get; set; }
		[DataMember]
		[DLB5("SHAREHOLDERKGEN", "Kgen name")]
		public string KgenName { get; set; }
		[DataMember]
		[DLB5("SHAREHOLDERREGNUM", "Shareholder registered number")]
		public string ShareholderRegisteredNumber { get; set; }
		[DataMember]
		[DLB5("ADDRLINE1", "Shareholder address")]
		public string AddressLine1 { get; set; }
		[DataMember]
		[DLB5("ADDRLINE2", "Shareholder address")]
		public string AddressLine2 { get; set; }
		[DataMember]
		[DLB5("ADDRLINE3", "Shareholder address")]
		public string AddressLine3 { get; set; }
		[DataMember]
		[DLB5("TOWN", "Shareholder address")]
		public string Town { get; set; }
		[DataMember]
		[DLB5("COUNTY", "Shareholder address")]
		public string County { get; set; }
		[DataMember]
		[DLB5("POSTCODE", "Shareholder address")]
		public string Postcode { get; set; }
		[DataMember]
		[DLB5("COUNTRYOFORIGIN", "Shareholder country")]
		public string Country { get; set; }
		[DataMember]
		[DLB5("PUNAPOSTCODE", "Shareholder puna pcode ")]
		public string ShareholderPunaPcode { get; set; }
		[DataMember]
		[DLB5("RMC", "Shareholder RMC")]
		public string ShareholderRMC { get; set; }
		[DataMember]
		[DLB5("SUPPRESS", "Suppression flag")]
		public string SuppressionFlag { get; set; }
		[DataMember]
		[DLB5("NOCREF", "NOC ref number")]
		public string NOCRefNumber { get; set; }
		[DataMember]
		[DLB5("LASTUPDATEDDATE-YYYY", "Last Updated")]
		public DateTime? LastUpdated { get; set; }
	} // class ExperianLtdDLB5
} // namespace
