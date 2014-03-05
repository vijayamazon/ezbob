namespace EzService {
	using System.Runtime.Serialization;

	#region class BoolActionResult

	[DataContract]
	public class BoolActionResult : ActionResult {
		[DataMember]
		public bool Value { get; set; }
	} // class BoolActionResult

	#endregion class BoolActionResult
} // namespace EzService
