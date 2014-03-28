namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class BrokerSmsCountActionResult : ActionResult {
		[DataMember]
		public int MaxPerNumber { get; set; }

		[DataMember]
		public int MaxPerPage { get; set; }
	} // class BrokerSmsCountActionResult
} // namespace EzService
