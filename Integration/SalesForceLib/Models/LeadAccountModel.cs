namespace SalesForceLib.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class LeadAccountModel {
		[DataMember]
		public string Email { get; set; } // lead/account unique identifier
		[DataMember]
		public string Origin { get; set; } // lead/account/opportunity unique identifier
		//----------------------------------------//

		//Contact Data
		[DataMember]
		public string CustomerID { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Gender { get; set; }
		[DataMember]
		public string PhoneNumber { get; set; }
		[DataMember]
		public string MobilePhoneNumber { get; set; }
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
        public bool IsTest { get; set; }
		[DataMember]
		public string CollectionStatus { get; set; }
		[DataMember]
		public string ExternalCollectionStatus { get; set; }

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
		public string EzbobStatus { get; set; } 
		[DataMember]
		public string EzbobSource { get; set; } 
		[DataMember]
		public string LeadSource { get; set; }
		[DataMember]
		public int RequestedLoanAmount { get; set; }
		[DataMember]
		public int? NumOfLoans { get; set; }
		[DataMember]
		public string Promocode { get; set; }

		//broker data
		[DataMember]
		public string BrokerName { get; set; }
		[DataMember]
		public string BrokerFirmName { get; set; }
		[DataMember]
		public string BrokerPhoneNumber { get; set; }
		[DataMember]
		public string BrokerEmail { get; set; }
		[DataMember]
		public int? BrokerID { get; set; }
	}
}
