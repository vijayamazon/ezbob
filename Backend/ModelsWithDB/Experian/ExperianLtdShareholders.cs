namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System.Runtime.Serialization;
	using Logger;

	[DataContract]
	[Sharehlds]
	public class ExperianLtdShareholders : AExperianLtdDataRow {
		public ExperianLtdShareholders(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[Sharehlds("SHLDNAME")]
		public string DescriptionOfShareholder { get; set; }

		[DataMember]
		[Sharehlds("SHLDHOLDING")]
		public string DescriptionOfShareholding { get; set; }

		[DataMember]
		[Sharehlds("SHLDREGNUM")]
		public string RegisteredNumberOfALimitedCompanyWhichIsAShareholder { get; set; }
	} // class ExperianLtdShareholders
} // namespace