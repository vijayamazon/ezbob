namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.Experian;

	[DataContract]
	public class ExperianConsumerActionResult : ActionResult {
		[DataMember]
		public ExperianConsumerData Value { get; set; }
	} // class ExperianLtdActionResult
} // namespace
