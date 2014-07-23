namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Attributes;
	using Logger;

	[DataContract]
	[DL52]
	public class ExperianLtdDL52 : AExperianLtdDataRow {
		public ExperianLtdDL52(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[DL52("RECORDTYPE", "Notice Type", @"{
			""K"": ""Intention to dissolve"",
			""L"": ""Company dissolved"",
			""M"": ""Company reinstated""
		}")]
		public string NoticeType { get; set; }

		[DataMember]
		[DL52("DATEOFNOTICE-YYYY", "Date of Notice")]
		public DateTime? DateOfNotice { get; set; }
	} // class ExperianLtdDL52
} // namespace
