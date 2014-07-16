namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.Experian;

	[DataContract]
	public class ExperianLtdActionResult : ActionResult {
		[DataMember]
		public ExperianLtd Value { get; set; }
	} // class ExperianLtdActionResult
} // namespace
