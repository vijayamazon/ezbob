namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Runtime.Serialization;
	using Attributes;
	using Logger;

	[DataContract]
	[DLA2]
	public class ExperianLtdDLA2 : AExperianLtdDataRow {
		public ExperianLtdDLA2(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[DLA2("DATEACCS-YYYY", "Date")]
		public DateTime? Date { get; set; }

		[DataMember]
		[DLA2("NUMEMPS", "Number of Employees")]
		public int? NumberOfEmployees { get; set; }
	} // class ExperianLtdDLA2
} // namespace
