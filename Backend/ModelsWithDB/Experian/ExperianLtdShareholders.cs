namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System.Runtime.Serialization;
	using Attributes;
	using Logger;

	[DataContract]
	[Sharehlds]
	public class ExperianLtdShareholders : AExperianLtdDataRow {
		public ExperianLtdShareholders(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[Sharehlds("SHLDNAME", "Description of Shareholder")]
		public string DescriptionOfShareholder { get; set; }

		[DataMember]
		[Sharehlds("SHLDHOLDING", "Description of Shareholding", Transformation = TransformationType.Shares)]
		public string DescriptionOfShareholding { get; set; }

		[DataMember]
		[Sharehlds("SHLDREGNUM", "Registered number of a limited company which is a shareholder")]
		public string RegisteredNumberOfALimitedCompanyWhichIsAShareholder { get; set; }
	} // class ExperianLtdShareholders
} // namespace