namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using System.Xml;
	using Logger;

	[DataContract]
	[SummaryLine]
	public class ExperianLtdCreditSummary : AExperianLtdDataRow {
		public ExperianLtdCreditSummary(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {} // constructor

		[DataMember]
		[SummaryLine("CREDTYPE")]
		public string CreditEventType { get; set; }

		[DataMember]
		[SummaryLine("TYPEDATE-YYYY")]
		public DateTime? DateOfMostRecentRecordForType { get; set; }
	} // class ExperianLtdCreditSummary
} // namespace
