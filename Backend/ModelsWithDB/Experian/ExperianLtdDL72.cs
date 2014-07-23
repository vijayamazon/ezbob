namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Attributes;
	using Logger;

	[DataContract]
	[DL72]
	public class ExperianLtdDL72 : AExperianLtdDataRow {
		public ExperianLtdDL72(ASafeLog oLog = null) : base(oLog) { } // constructor

		[DataMember]
		[DL72("FOREIGNFLAG", "Foreign Address Flag", @"{
			""Y"": ""Foreign"",
			""N"": ""Not foreign""
		}")]
		public string ForeignAddressFlag { get; set; }

		[DataMember]
		[DL72("DIRCOMPFLAG", "Director Is a Company Flag", @"{
			""Y"": ""Director is a company"",
			""N"": ""Director is not a company""
		}")]
		public string IsCompany { get; set; }

		[DataMember]
		[DL72("DIRNUMBER", "Director number")]
		public string Number { get; set; }

		[DataMember]
		[DL72("DIRSHIPLEN", "Length of directorship", Transformation = TransformationType.MonthsAndYears)]
		public int? LengthOfDirectorship { get; set; }

		[DataMember]
		[DL72("DIRAGE", "Director's age (Years)")]
		public int? DirectorsAgeYears { get; set; }

		[DataMember]
		[DL72("NUMCONVICTIONS", "Number of convictions")]
		public int? NumberOfConvictions { get; set; }

		[DataMember]
		[DL72("DIRNAMEPREFIX", "Name")]
		public string Prefix { get; set; }

		[DataMember]
		[DL72("DIRFORENAME", "Name", DisplayPrefix = " ")]
		public string FirstName { get; set; }

		[DataMember]
		[DL72("DIRMIDNAME1", "Name", DisplayPrefix = " ")]
		public string MidName1 { get; set; }

		[DataMember]
		[DL72("DIRMIDNAME2", "Name", DisplayPrefix = " ")]
		public string MidName2 { get; set; }

		[DataMember]
		[DL72("DIRSURNAME", "Name", DisplayPrefix = " ")]
		public string LastName { get; set; }

		[DataMember]
		[DL72("DIRNAMESUFFIX", "Name", DisplayPrefix = " ")]
		public string Suffix { get; set; }

		[DataMember]
		[DL72("DIRQUALS", "Qualifications")]
		public string Qualifications { get; set; }

		[DataMember]
		[DL72("DIRTITLE", "Director title")]
		public string Title { get; set; }

		[DataMember]
		[DL72("DIRCOMPNAME", "Company name")]
		public string CompanyName { get; set; }

		[DataMember]
		[DL72("DIRCOMPNUM", "Company number")]
		public string CompanyNumber { get; set; }

		[DataMember]
		[DL72("DIRSHAREINFO", IsCompanyScoreModel = false)]
		public string ShareInfo { get; set; }

		[DataMember]
		[DL72("DATEOFBIRTH-YYYY", IsCompanyScoreModel = false)]
		public DateTime? BirthDate { get; set; }

		[DataMember]
		[DL72("DIRHOUSENAME", IsCompanyScoreModel = false)]
		public string HouseName { get; set; }

		[DataMember]
		[DL72("DIRHOUSENUM", IsCompanyScoreModel = false)]
		public string HouseNumber { get; set; }

		[DataMember]
		[DL72("DIRSTREET", IsCompanyScoreModel = false)]
		public string Street { get; set; }

		[DataMember]
		[DL72("DIRTOWN", IsCompanyScoreModel = false)]
		public string Town { get; set; }

		[DataMember]
		[DL72("DIRCOUNTY", IsCompanyScoreModel = false)]
		public string County { get; set; }

		[DataMember]
		[DL72("DIRPOSTCODE", IsCompanyScoreModel = false)]
		public string Postcode { get; set; }
	} // class ExperianLtdDL72
} // namespace
