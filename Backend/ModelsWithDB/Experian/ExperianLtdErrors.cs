namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System.Runtime.Serialization;
	using Logger;

	[DataContract]
	[ERR1]
	public class ExperianLtdErrors : AExperianLtdDataRow {
		public ExperianLtdErrors(ASafeLog oLog = null) : base(oLog) {} // constructor

		[DataMember]
		[ERR1("MESSAGE")]
		public string ErrorMessage { get; set; }
	} // class ExperianLtdErrors
} // namespace
