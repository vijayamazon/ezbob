using System;

namespace EZBob.DatabaseLib.DatabaseWrapper.AccountInfo
{
	public class PayPalPersonalData
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string FullName { get; set; }
		public string BusinessName { get; set; }
		public string PlayerId { get; set; }
		public DateTime? BirthDate { get; set; }
		public string AddressCountry { get; set; }
		public string AddressPostCode { get; set; }
		public string AddressStreet1 { get; set; }
		public string AddressStreet2 { get; set; }
		public string AddressCity { get; set; }
		public string AddressState { get; set; }
		public string Phone { get; set; }

		public DateTime SubmittedDate { get; set; }
	}
}
