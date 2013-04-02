using System;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_PayPalPersonalInfo
	{
		public virtual int Id { get; set; }

		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Updated { get; set; }

		public virtual string FirstName { get; set; }
		public virtual string LastName { get; set; }
		public virtual string EMail { get; set; }
		public virtual string FullName { get; set; }
		public virtual string BusinessName { get; set; }
		public virtual string Country { get; set; }
		public virtual string PlayerId { get; set; }
		public virtual string Postcode { get; set; }
		public virtual string Street1 { get; set; }
		public virtual string Street2 { get; set; }
		public virtual string City { get; set; }
		public virtual string State { get; set; }
		public virtual string Phone { get; set; }
		public virtual DateTime? DateOfBirth { get; set; }
	}
}