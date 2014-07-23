namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System.Runtime.Serialization;
	using Attributes;
	using Logger;

	[DataContract]
	[DL48]
	public class ExperianLtdDL48 : AExperianLtdDataRow {
		public ExperianLtdDL48(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[DL48("FRAUDCATEGORY", "Fraud Category", @"{
			""01"": ""Providing a false name and a true address."",
			""02"": ""Providing or using the name and particulars of another person."",
			""03"": ""Providing or using a genuine name and address, but one or more material falsehoods in personal details followed by a serious misuse of the credit or other facility and/or non-payment."",
			""04"": ""Providing or using a genuine name and address, but one or more material falsehoods in personal details."",
			""05"": ""Disposal/selling on of goods obtained on credit and failing to settle the finance agreement."",
			""06"": ""Opening an account for the purpose of fraud.""
		}")]
		public string FraudCategory { get; set; }

		[DataMember]
		[DL48("SUPPLIERNAME", "Supplier Name")]
		public string SupplierName { get; set; }
	} // class ExperianLtdDL48
} // namespace
