namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils;

	#region class BrokerProperties

	[DataContract]
	public class BrokerProperties : ITraversable {
		[DataMember]
		public int BrokerID { get; set; }

		[DataMember]
		public string BrokerName { get; set; }

		[DataMember]
		public string BrokerRegNum { get; set; }

		[DataMember]
		public string ContactName { get; set; }

		[DataMember]
		public string ContactEmail { get; set; }

		[DataMember]
		public string ContactMobile { get; set; }

		[DataMember]
		public string ContactOtherPhone { get; set; }

		[DataMember]
		public string SourceRef { get; set; }

		[DataMember]
		public string BrokerWebSiteUrl { get; set; }
	} // class BrokerProperties

	#endregion class BrokerProperties
} // namespace EzBob.Backend.Strategies.Broker
