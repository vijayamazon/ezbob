namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Logger;

	[DataContract]
	[DLA2]
	public class ExperianLtdDLA2 : AExperianLtdDataRow {
		public ExperianLtdDLA2(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[DLA2("DATEACCS-YYYY")]
		public DateTime? Date { get; set; }

		[DataMember]
		[DLA2("NUMEMPS")]
		public int? NumberOfEmployees { get; set; }
	} // class ExperianLtdDLA2
} // namespace
