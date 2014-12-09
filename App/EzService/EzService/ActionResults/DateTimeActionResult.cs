namespace EzService {
	using System;
	using System.Runtime.Serialization;

	[DataContract]
	public class DateTimeActionResult : ActionResult {
		[DataMember]
		public DateTime Value { get; set; }
	} // class DateTimeActionResult

} // namespace EzService
