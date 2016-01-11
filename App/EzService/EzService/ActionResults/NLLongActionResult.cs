namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class NLLongActionResult : ActionResult {
		[DataMember]
		public long Value { get; set; }

		[DataMember]
		public string Error { get; set; }
	} // class NLLongActionResult
} // namespace
