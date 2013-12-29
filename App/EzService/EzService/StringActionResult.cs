namespace EzService {
	using System.Runtime.Serialization;

	#region class StringActionResult

	[DataContract]
	public class StringActionResult : ActionResult
	{
		[DataMember]
		public string Value { get; set; }
	} // class StringActionResult

	#endregion class StringActionResult
} // namespace EzService
