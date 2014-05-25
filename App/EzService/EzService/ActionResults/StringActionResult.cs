namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class StringActionResult : ActionResult {
		[DataMember]
		public string Value { get; set; }
	} // class StringActionResult
} // namespace EzService
