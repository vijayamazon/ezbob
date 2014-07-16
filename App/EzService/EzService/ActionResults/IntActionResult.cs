namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class IntActionResult : ActionResult {
		[DataMember]
		public int Value { get; set; }
	} // class IntActionResult
} // namespace
