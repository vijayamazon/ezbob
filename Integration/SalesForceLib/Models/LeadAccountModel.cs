namespace SalesForceLib.Models {
	using System;
	using System.Runtime.Serialization;

	//todo use auto generated object from sales force
	[DataContract]
	public class LeadAccountModel {
		[DataMember]
		public string Email { get; set; }
		//----------------------------------------//

		//Contact Data
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
		[DataMember]
		public bool IsBroker { get; set; }
		[DataMember]
		public DateTime? RegistrationDate { get; set; }
		[DataMember]
		public string Origin { get; set; }
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
	}
}
