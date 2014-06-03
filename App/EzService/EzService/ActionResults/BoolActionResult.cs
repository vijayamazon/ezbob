namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class BoolActionResult : ActionResult {
		[DataMember]
		public bool Value { get; set; }
	} // class BoolActionResult
} // namespace EzService
