namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Attributes;
	using Logger;

	[DataContract]
	[SummaryLine]
	public class ExperianLtdCreditSummary : AExperianLtdDataRow {
		public ExperianLtdCreditSummary(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[SummaryLine("CREDTYPE", "Credit Event Type", @"{
			""C"": ""Receiver appointments"",
			""D"": ""Cessations of Receiver"",
			""E"": ""Winding up petitions"",
			""F"": ""Dismissals of winding up petitions"",
			""G"": ""Winding up orders"",
			""H"": ""Voluntary appointments of liquidators"",
			""I"": ""Meetings of creditors"",
			""J"": ""Resolutions to wind up"",
			""K"": ""Intentions to dissolve"",
			""L"": ""Dissolution notices"",
			""M"": ""Reinstatement notices"",
			""Q"": ""Administrators appointed"",
			""R"": ""Administrators dismissals"",
			""S"": ""Approvals of Voluntary arrangements"",
			""T"": ""Completions of Voluntary arrangements"",
			""U"": ""Compulsory appointments of liquidators"",
			""V"": ""Revocations of Voluntary arrangements"",
			""W"": ""Suspensions of Voluntary arrangements""
		}")]
		public string CreditEventType { get; set; }

		[DataMember]
		[SummaryLine("TYPEDATE-YYYY", "Date of Most Recent Record for Type")]
		public DateTime? DateOfMostRecentRecordForType { get; set; }
	} // class ExperianLtdCreditSummary
} // namespace
