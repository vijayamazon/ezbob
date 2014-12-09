namespace EzService {
	using Ezbob.Backend.Models;
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class BrokerCustomersActionResult : ActionResult {
		[DataMember]
		public List<BrokerCustomerEntry> Customers { get; set; }
	} // class BrokerCustomersActionResult

} // namespace EzService
