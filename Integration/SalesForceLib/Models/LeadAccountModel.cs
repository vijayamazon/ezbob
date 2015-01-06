namespace SalesForceLib.Models {
	using System;

	//todo use auto generated object from sales force
	public class LeadAccountModel {
		public string Email { get; set; }
		//----------------------------------------//

		//Contact Data
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
		public bool IsBroker { get; set; }
		public DateTime? RegistrationDate { get; set; }

		//Company fields
		public string CompanyName { get; set; }
		public string CompanyNumber { get; set; }
		public string TypeOfBusiness { get; set; }
		public string Industry { get; set; }

		//state source
		public string EzbobStatus { get; set; }
		public string EzbobSource { get; set; }
		public string LeadSource { get; set; }
		public int RequestedLoanAmount { get; set; }
		
	}
}
