namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class LongActionResult : ActionResult {
		[DataMember]
		public long Value { get; set; }
	} // class LongActionResult
} // namespace
