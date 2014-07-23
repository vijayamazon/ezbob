namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Attributes;
	using Logger;

	[DataContract]
	[PrevCompNames]
	public class ExperianLtdPrevCompanyNames : AExperianLtdDataRow {
		public ExperianLtdPrevCompanyNames(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[PrevCompNames("DATECHANGED", "Date Changed")]
		public DateTime? DateChanged { get; set; }

		[DataMember]
		[PrevCompNames("PREVREGADDR1", "Office Address")]
		public string OfficeAddress1 { get; set; }

		[DataMember]
		[PrevCompNames("PREVREGADDR2", "Office Address")]
		public string OfficeAddress2 { get; set; }

		[DataMember]
		[PrevCompNames("PREVREGADDR3", "Office Address")]
		public string OfficeAddress3 { get; set; }

		[DataMember]
		[PrevCompNames("PREVREGADDR4", "Office Address")]
		public string OfficeAddress4 { get; set; }

		[DataMember]
		[PrevCompNames("PREVREGPOSTCODE", "Office Address")]
		public string OfficeAddressPostcode { get; set; }
	} // class ExperianLtdPrevCompanyNames
} // namespace
