namespace EzService {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using System.Runtime.Serialization;

	[DataContract]
	public class BrokerCustomerDetailsActionResult : ActionResult {
		[DataMember]
		public BrokerCustomerDetails Data { get; set; }

		[DataMember]
		public IEnumerable<Esigner> PotentialSigners { get; set; }
	} // class BrokerCustomerDetailsActionResult
} // namespace EzService
