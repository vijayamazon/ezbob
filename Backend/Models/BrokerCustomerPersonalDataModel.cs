namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class BrokerCustomerPersonalData {
		[DataMember]
		public int id { get; set; }

		[DataMember]
		public string name { get; set; }

		[DataMember]
		public DateTime birthdate { get; set; }

		[DataMember]
		public string gender { get; set; }

		[DataMember]
		public string email { get; set; }

		[DataMember]
		public string maritalstatus { get; set; }

		[DataMember]
		public string mobilephone { get; set; }

		[DataMember]
		public string daytimephone { get; set; }

		[DataMember]
		public string address { get; set; }

        [DataMember]
        public int leadID { get; set; }

        [DataMember]
        public bool finishedWizard { get; set; }
	} // class BrokerCustomerPersonalData

} // namespace Ezbob.Backend.Strategies.Broker
