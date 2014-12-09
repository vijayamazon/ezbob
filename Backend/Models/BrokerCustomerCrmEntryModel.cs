namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class BrokerCustomerCrmEntry {
		[DataMember]
		public DateTime CrDate { get; set; }

		[DataMember]
		public string ActionName { get; set; }

		[DataMember]
		public string StatusName { get; set; }

		[DataMember]
		public string Comment { get; set; }
	} // class BrokerCustomerCrmEntry
} // namespace Ezbob.Backend.Strategies.Broker
