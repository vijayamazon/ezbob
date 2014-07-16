namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using System.Xml;
	using Logger;

	[DataContract]
	[DL72]
	public class ExperianLtdDL72 : AExperianLtdDataRow {
		public ExperianLtdDL72(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) { } // constructor

		[DataMember]
		[DL72("FOREIGNFLAG")]
		public string ForeignAddressFlag { get; set; }
		[DataMember]
		[DL72("DIRCOMPFLAG")]
		public string IsCompany { get; set; }
		[DataMember]
		[DL72("DIRNUMBER")]
		public string Number { get; set; }
		[DataMember]
		[DL72("DIRSHIPLEN")]
		public int? LengthOfDirectorship { get; set; }
		[DataMember]
		[DL72("DIRAGE")]
		public int? DirectorsAgeYears { get; set; }
		[DataMember]
		[DL72("NUMCONVICTIONS")]
		public int? NumberOfConvictions { get; set; }
		[DataMember]
		[DL72("DIRNAMEPREFIX")]
		public string Prefix { get; set; }
		[DataMember]
		[DL72("DIRFORENAME")]
		public string FirstName { get; set; }
		[DataMember]
		[DL72("DIRMIDNAME1")]
		public string MidName1 { get; set; }
		[DataMember]
		[DL72("DIRMIDNAME2")]
		public string MidName2 { get; set; }
		[DataMember]
		[DL72("DIRSURNAME")]
		public string LastName { get; set; }
		[DataMember]
		[DL72("DIRNAMESUFFIX")]
		public string Suffix { get; set; }
		[DataMember]
		[DL72("DIRQUALS")]
		public string Qualifications { get; set; }
		[DataMember]
		[DL72("DIRTITLE")]
		public string Title { get; set; }
		[DataMember]
		[DL72("DIRCOMPNAME")]
		public string CompanyName { get; set; }
		[DataMember]
		[DL72("DIRCOMPNUM")]
		public string CompanyNumber { get; set; }
		[DataMember]
		[DL72("DIRSHAREINFO")]
		public string ShareInfo { get; set; }
		[DataMember]
		[DL72("DATEOFBIRTH-YYYY")]
		public DateTime? BirthDate { get; set; }
		[DataMember]
		[DL72("DIRHOUSENAME")]
		public string HouseName { get; set; }
		[DataMember]
		[DL72("DIRHOUSENUM")]
		public string HouseNumber { get; set; }
		[DataMember]
		[DL72("DIRSTREET")]
		public string Street { get; set; }
		[DataMember]
		[DL72("DIRTOWN")]
		public string Town { get; set; }
		[DataMember]
		[DL72("DIRCOUNTY")]
		public string County { get; set; }
		[DataMember]
		[DL72("DIRPOSTCODE")]
		public string Postcode { get; set; }
	} // class ExperianLtdDL72
} // namespace
