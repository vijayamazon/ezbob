namespace SalesForceLib.Models {
	using System;
	using System.ComponentModel;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class LeadAccountModel {
		[DataMember]
		public string Email { get; set; } // lead/account unique identifier
		//----------------------------------------//

		//Contact Data
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Gender { get; set; } //Gender enum
		[DataMember]
		public string PhoneNumber { get; set; }
		[DataMember]
		public DateTime? DateOfBirth { get; set; }
		[DataMember]
		public string AddressLine1 { get; set; }
		[DataMember]
		public string AddressLine2 { get; set; }
		[DataMember]
		public string AddressLine3 { get; set; }
		[DataMember]
		public string AddressPostcode { get; set; }
		[DataMember]
		public string AddressTown { get; set; }
		[DataMember]
		public string AddressCounty { get; set; }
		[DataMember]
		public string AddressCountry { get; set; }
		[DataMember]
		public bool IsBroker { get; set; }
		[DataMember]
		public DateTime? RegistrationDate { get; set; }
		[DataMember]
		public string Origin { get; set; }
        [DataMember]
        public bool IsTest { get; set; }
		//Company data
		[DataMember]
		public string CompanyName { get; set; }
		[DataMember]
		public string CompanyNumber { get; set; }
		[DataMember]
		public string TypeOfBusiness { get; set; }
		[DataMember]
		public string Industry { get; set; }

		//state source data
		[DataMember]
		public string EzbobStatus { get; set; } //EzbobStatus enum
		[DataMember]
		public string EzbobSource { get; set; } //EzbobSource enum
		[DataMember]
		public string LeadSource { get; set; }
		[DataMember]
		public int RequestedLoanAmount { get; set; }
	}

    public enum EzbobSource
    {
        [Description("Wizard")]
        Wizard,
        [Description("Wizard")]
        VIP,
        [Description("Landing page")]
        LandingPage,
        [Description("Broker lead")]
        BrokerLead
    }

    public enum EzbobStatus
    {
        Contacted,
        Open,
        Qualified,
        Unqualified
    }

    public enum Gender
    {
        Male,
        Female
    }


}
