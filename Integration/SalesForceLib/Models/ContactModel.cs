namespace SalesForceLib.Models {
	using System;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class ContactModel {
		[DataMember]
		public string Email { get; set; } // account unique identifier
		[DataMember]
		public string Origin { get; set; } // lead/account/opportunity unique identifier
		//----------------------------------------//
		[DataMember]
		public string ContactEmail { get; set; } 
		[DataMember]
		public string Type { get; set; } //ContactType enum
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Gender { get; set; }
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
	}

	public enum ContactType {
		MainApplicant,
		Director
	}

}
