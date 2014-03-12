namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils;

	[DataContract]
	public class BrokerLeadEntry : ITraversable {
		[DataMember]
		public int LeadID { get; set; }

		[DataMember]
		public string FirstName { get; set; }

		[DataMember]
		public string LastName { get; set; }

		[DataMember]
		public string Email { get; set; }

		[DataMember]
		public string AddMode { get; set; }

		[DataMember]
		public DateTime DateCreated { get; set; }

		[DataMember]
		public DateTime DateLastInvitationSent { get; set; }
	} // class BrokerLeadEntry
} // namespace EzBob.Backend.Strategies.Broker
