﻿namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;
	using Utils;

	#region class BrokerCustomerCrmEntry

	[DataContract]
	public class BrokerCustomerCrmEntry : ITraversable {
		[DataMember]
		public DateTime CrDate { get; set; }

		[DataMember]
		public string ActionName { get; set; }

		[DataMember]
		public string StatusName { get; set; }

		[DataMember]
		public string Comment { get; set; }
	} // class BrokerCustomerCrmEntry

	#endregion class BrokerCustomerCrmEntry
} // namespace EzBob.Backend.Strategies.Broker
