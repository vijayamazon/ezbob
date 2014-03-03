namespace EzService {
	using Ezbob.Backend.Models;
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	#region class BrokerCustomersActionResult

	[DataContract]
	public class BrokerCustomersActionResult : ActionResult {
		[DataMember]
		public List<BrokerCustomerEntry> Records { get; set; }
	} // class BrokerCustomersActionResult

	#endregion class BrokerCustomersActionResult
} // namespace EzService
