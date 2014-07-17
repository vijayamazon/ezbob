namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System.Runtime.Serialization;
	using Logger;

	[DataContract]
	[DL48]
	public class ExperianLtdDL48 : AExperianLtdDataRow {
		public ExperianLtdDL48(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[DL48("FRAUDCATEGORY")]
		public string FraudCategory { get; set; }

		[DataMember]
		[DL48("SUPPLIERNAME")]
		public string SupplierName { get; set; }
	} // class ExperianLtdDL48
} // namespace
