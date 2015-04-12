namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
    public class BrokerLeadDataModel {
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public DateTime DateCreated { get; set; }

        [DataMember]
        public DateTime? DateLastInvitationSent { get; set; }

		[DataMember]
		public string Email { get; set; }
	} // class BrokerLeadData

} // namespace Ezbob.Backend.Strategies.Broker
