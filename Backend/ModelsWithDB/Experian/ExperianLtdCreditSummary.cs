namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Logger;

	[DataContract]
	[SummaryLine]
	public class ExperianLtdCreditSummary : AExperianLtdDataRow {
		public ExperianLtdCreditSummary(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[SummaryLine("CREDTYPE")]
		public string CreditEventType { get; set; }

		[DataMember]
		[SummaryLine("TYPEDATE-YYYY")]
		public DateTime? DateOfMostRecentRecordForType { get; set; }
	} // class ExperianLtdCreditSummary
} // namespace
