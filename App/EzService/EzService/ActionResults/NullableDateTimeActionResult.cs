namespace EzService {
	using System;
	using System.Runtime.Serialization;

	#region class NullableDateTimeActionResult

	[DataContract]
	public class NullableDateTimeActionResult : ActionResult
	{
		[DataMember]
		public DateTime? Value { get; set; }
	} // class NullableDateTimeActionResult

	#endregion class NullableDateTimeActionResult
} // namespace EzService
