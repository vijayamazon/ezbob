namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Logger;

	[DataContract]
	[PrevCompNames]
	public class ExperianLtdPrevCompanyNames : AExperianLtdDataRow {
		public ExperianLtdPrevCompanyNames(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[PrevCompNames("DATECHANGED")]
		public DateTime? DateChanged { get; set; }

		[DataMember]
		[PrevCompNames("PREVREGADDR1")]
		public string OfficeAddress1 { get; set; }

		[DataMember]
		[PrevCompNames("PREVREGADDR2")]
		public string OfficeAddress2 { get; set; }

		[DataMember]
		[PrevCompNames("PREVREGADDR3")]
		public string OfficeAddress3 { get; set; }

		[DataMember]
		[PrevCompNames("PREVREGADDR4")]
		public string OfficeAddress4 { get; set; }

		[DataMember]
		[PrevCompNames("PREVREGPOSTCODE")]
		public string OfficeAddressPostcode { get; set; }
	} // class ExperianLtdPrevCompanyNames
} // namespace
