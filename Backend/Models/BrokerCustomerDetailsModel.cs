namespace Ezbob.Backend.Models {
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class BrokerCustomerDetails {
		public BrokerCustomerDetails() {
			CrmData = new List<BrokerCustomerCrmEntry>();
			PersonalData = new BrokerCustomerPersonalData();
		} // constructor

		[DataMember]
		public List<BrokerCustomerCrmEntry> CrmData { get; set; }

		[DataMember]
		public BrokerCustomerPersonalData PersonalData { get; set; }
	} // class BrokerCustomerDetails
} // namespace Ezbob.Backend.Strategies.Broker
