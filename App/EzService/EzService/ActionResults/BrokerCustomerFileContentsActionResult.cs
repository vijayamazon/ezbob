namespace EzService {
	using System.Runtime.Serialization;

	#region class BrokerCustomerFileContentsActionResult

	public class BrokerCustomerFileContentsActionResult : ActionResult {
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public byte[] Contents { get; set; }
	} // class BrokerCustomerFileContentsActionResult

	#endregion class BrokerCustomerFileContentsActionResult
} // namespace EzService
