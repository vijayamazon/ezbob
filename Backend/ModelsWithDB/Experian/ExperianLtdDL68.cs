namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System.Runtime.Serialization;
	using Logger;

	[DataContract]
	[DL68]
	public class ExperianLtdDL68 : AExperianLtdDataRow {
		public ExperianLtdDL68(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[DL68("SUBSIDREGNUM")]
		public string SubsidiaryRegisteredNumber { get; set; }

		[DataMember]
		[DL68("SUBSIDSTATUS")]
		public string SubsidiaryStatus { get; set; }

		[DataMember]
		[DL68("SUBSIDLEGALSTATUS")]
		public string SubsidiaryLegalStatus { get; set; }

		[DataMember]
		[DL68("SUBSIDNAME")]
		public string SubsidiaryName { get; set; }
	} // class ExperianLtdDL68
} // namespace
