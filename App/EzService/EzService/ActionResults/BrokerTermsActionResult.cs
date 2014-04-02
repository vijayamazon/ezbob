namespace EzService {
	using System.Runtime.Serialization;

	[DataContract]
	public class BrokerTermsActionResult : ActionResult {
		[DataMember]
		public int TermsID { get; set; }

		[DataMember]
		public string Terms { get; set; }
	} // class BrokerTermsActionResult
} // namespace EzBob.Backend.Strategies.Broker
