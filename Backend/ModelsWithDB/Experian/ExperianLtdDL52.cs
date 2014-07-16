namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using System.Xml;
	using Logger;

	[DataContract]
	[DL52]
	public class ExperianLtdDL52 : AExperianLtdDataRow {
		public ExperianLtdDL52(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[DataMember]
		[DL52("RECORDTYPE")]
		public string NoticeType { get; set; }

		[DataMember]
		[DL52("DATEOFNOTICE-YYYY")]
		public DateTime? DateOfNotice { get; set; }
	} // class ExperianLtdDL52
} // namespace
