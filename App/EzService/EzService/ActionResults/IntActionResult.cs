namespace EzService {
	using System.Runtime.Serialization;

	#region class IntActionResult

	[DataContract]
	public class IntActionResult : ActionResult {
		[DataMember]
		public int Value { get; set; }
	} // class IntActionResult

	#endregion class IntActionResult
} // namespace EzService
