namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System.Runtime.Serialization;
	using Attributes;
	using Logger;

	[DataContract]
	[CaisMonthly]
	public class ExperianLtdCaisMonthly : AExperianLtdDataRow {
		public ExperianLtdCaisMonthly(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[CaisMonthly("NUMACTIVEACCS")]
		public decimal? NumberOfActiveAccounts { get; set; }
	} // class ExperianLtdCaisMonthly
} // namespace
