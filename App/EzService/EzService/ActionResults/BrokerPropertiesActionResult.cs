namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models;

	[DataContract]
	public class BrokerPropertiesActionResult : ActionResult {
		[DataMember]
		public BrokerProperties Properties { get; set; }
	} // class BrokerPropertiesActionResult
} // namespace EzService
