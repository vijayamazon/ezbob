namespace SalesForceLib.Models {
	using System;
	using System.Runtime.Serialization;

	//todo use auto generated object from sales force
	[DataContract]
	public class ContactModel {
		[DataMember]
		public string Email { get; set; }
		//----------------------------------------//
		[DataMember]
		public string ContactEmail { get; set; }
		[DataMember]
		public string Type { get; set; }
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
