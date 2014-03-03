namespace EzService {
	using Ezbob.Backend.Models;
	using System.Runtime.Serialization;

	#region class BrokerCustomerDetailsActionResult

	[DataContract]
	public class BrokerCustomerDetailsActionResult : ActionResult {
		[DataMember]
		public BrokerCustomerDetails Data { get; set; }
	} // class BrokerCustomerDetailsActionResult

	#endregion class BrokerCustomerDetailsActionResult
} // namespace EzService
