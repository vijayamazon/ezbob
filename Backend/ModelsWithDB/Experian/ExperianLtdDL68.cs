namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System.Runtime.Serialization;
	using Attributes;
	using Logger;

	[DataContract]
	[DL68]
	public class ExperianLtdDL68 : AExperianLtdDataRow {
		public ExperianLtdDL68(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[DL68("SUBSIDREGNUM", "Subsidiary registered number")]
		public string SubsidiaryRegisteredNumber { get; set; }

		[DataMember]
		[DL68("SUBSIDSTATUS", "Subsidiary status", @"{
			""L"": ""Live"",
			""D"": ""Dormant"",
			""S"": ""Dissolved""
		}")]
		public string SubsidiaryStatus { get; set; }

		[DataMember]
		[DL68("SUBSIDLEGALSTATUS", "Subsidiary legal status", @"{
			""1"": ""Private Unlimited"",
			""2"": ""Private Limited"",
			""3"": ""PLC"",
			""4"": ""Old Public Company"",
			""5"": ""Private Company Limited by Guarantee (Exempt from using word Limited)"",
			""6"": ""Limited Partnership"",
			""7"": ""Private Limited Company Without Share Capital"",
			""8"": ""Company Converted / Closed"",
			""9"": ""Private Unlimited Company Without Share Capital"",
			""0"": ""Other"",
			""A"": ""Private Company Limited by Shares (Exempt from using word Limited)""
		}")]
		public string SubsidiaryLegalStatus { get; set; }

		[DataMember]
		[DL68("SUBSIDNAME", "Subsidiary name")]
		public string SubsidiaryName { get; set; }
	} // class ExperianLtdDL68
} // namespace
