namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Utils;

	[DataContract]
	public class ElapsedTimeInfoActionResult : ActionResult {
		[DataMember]
		public ElapsedTimeInfo Value { get; set; }
	} // class ElapsedTimeInfoActionResult
} // namespace EzService
