namespace EzService {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class NullableDateTimeActionResult : ActionResult
	{
		[DataMember]
		public DateTime? Value { get; set; }
	} // class NullableDateTimeActionResult

} // namespace EzService
