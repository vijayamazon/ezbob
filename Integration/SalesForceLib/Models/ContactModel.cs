namespace SalesForceLib.Models {
	using System;

	//todo use auto generated object from sales force
	public class ContactModel {
		public string Email { get; set; }
		//----------------------------------------//
		public string ContactEmail { get; set; }
		public string Type { get; set; }
		public string Name { get; set; }
		public string Gender { get; set; }
		public string PhoneNumber { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string AddressLine3 { get; set; }
		public string AddressPostcode { get; set; }
		public string AddressTown { get; set; }
		public string AddressCounty { get; set; }
		public string AddressCountry { get; set; }

	}

	public enum ContactType {
		MainApplicant,
		Director
	}

}
