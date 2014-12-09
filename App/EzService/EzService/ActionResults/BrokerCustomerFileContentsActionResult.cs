namespace EzService {
	using System.Runtime.Serialization;

	public class BrokerCustomerFileContentsActionResult : ActionResult {
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public byte[] Contents { get; set; }
	} // class BrokerCustomerFileContentsActionResult

} // namespace EzService
