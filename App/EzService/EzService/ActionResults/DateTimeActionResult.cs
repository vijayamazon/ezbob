namespace EzService {
	using System;
	using System.Runtime.Serialization;

	#region class DateTimeActionResult

	[DataContract]
	public class DateTimeActionResult : ActionResult {
		[DataMember]
		public DateTime Value { get; set; }
	} // class DateTimeActionResult

	#endregion class DateTimeActionResult
} // namespace EzService
