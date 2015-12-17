namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class BrokerCustomerEntry {

		public BrokerCustomerEntry() {
			LoanDate = ms_oLongTimeAgo;
		    CommissionPaymentDate = ms_oLongTimeAgo;
		} // constructor

		[DataMember]
		public int CustomerID { get; set; }

		[DataMember]
		public string RefNumber { get; set; }
        [DataMember]
        public bool IsWaitingForSignature { get; set; }
      

		[DataMember]
		public string FirstName { get; set; }

		[DataMember]
		public string LastName { get; set; }

		[DataMember]
		public string Email { get; set; }

		[DataMember]
		public string WizardStep { get; set; }

		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public DateTime ApplyDate { get; set; }

		[DataMember]
		public string Marketplaces { get; set; }

		[DataMember]
		public decimal LoanAmount { get; set; }

		[DataMember]
		public DateTime LoanDate { get; set; }

		[DataMember]
		public decimal SetupFee { get; set; }

		[DataMember]
		public int LeadID { get; set; }

		[DataMember]
		public bool IsLeadDeleted { get; set; }

		[DataMember]
		public DateTime LastInvitationSent { get; set; }

        [DataMember]
        public decimal ApprovedAmount { get; set; }

        [DataMember]
        public decimal CommissionAmount { get; set; }

        [DataMember]
        public DateTime CommissionPaymentDate { get; set; }
        
		public BrokerCustomerEntry SetLead(int nLeadID, bool bIsLeadDeleted, DateTime oLastInvitationSent, string sFirstName, string sLastName) {
			LeadID = nLeadID;
			IsLeadDeleted = bIsLeadDeleted;
			LastInvitationSent = oLastInvitationSent;

			if (string.IsNullOrWhiteSpace(FirstName))
				FirstName = sFirstName;

			if (string.IsNullOrWhiteSpace(LastName))
				LastName = sLastName;

			return this;
		} // SetLead

		private static readonly DateTime ms_oLongTimeAgo = new DateTime(1976, 7, 1).Date;
    } // class BrokerCustomerEntry
} // namespace Ezbob.Backend.Strategies.Broker
