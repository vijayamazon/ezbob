namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	#region class BrokerCustomerCrmEntry

	[DataContract]
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
	} // class BrokerCustomerPersonalData

	#endregion class BrokerCustomerPersonalData
} // namespace EzBob.Backend.Strategies.Broker
