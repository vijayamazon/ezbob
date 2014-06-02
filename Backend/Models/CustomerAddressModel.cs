﻿namespace Ezbob.Backend.Models
{
	public class CustomerAddressModel
	{
		public string Line1 { get; set; }
		public string Line2 { get; set; }
		public string Line3 { get; set; }
		public string City { get; set; }
		public string County { get; set; }
		public string FlatOrApartmentNumber { get; set; }
		public string HouseNumber { get; set; }
		public string HouseName { get; set; }
		public string Country { get; set; }
		public string PostCode { get; set; }
		public string POBox { get; set; }

		public string Address1 { get; set; }
		public string Address2 { get; set; }
	}

	public class CustomerAddressesModel
	{
		public CustomerAddressModel CurrentAddress { get; set; }
		public CustomerAddressModel PreviousAddress { get; set; }
	}
}
