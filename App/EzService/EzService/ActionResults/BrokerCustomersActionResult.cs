namespace EzService {
	using Ezbob.Backend.Models;
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	#region class BrokerCustomersActionResult

	[DataContract]
	public class BrokerCustomersActionResult : ActionResult {
		[DataMember]
		public List<BrokerCustomerEntry> Customers { get; set; }

		[DataMember]
		public List<BrokerLeadEntry> Leads { get; set; }
	} // class BrokerCustomersActionResult

	#endregion class BrokerCustomersActionResult
} // namespace EzService
