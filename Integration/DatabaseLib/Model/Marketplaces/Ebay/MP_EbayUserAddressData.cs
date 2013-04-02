namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayUserAddressData
	{
		public virtual int Id { get; set; }
		
		public virtual string AddressID { get; set; }
		public virtual string AddressOwner { get; set; }
		public virtual string AddressRecordType { get; set; }
		public virtual string AddressStatus { get; set; }
		public virtual string AddressUsage { get; set; }
		public virtual string CityName { get; set; }
		public virtual string CompanyName { get; set; }
		public virtual string CountryCode { get; set; }
		public virtual string CountryName { get; set; }
		public virtual string County { get; set; }
		public virtual string ExternalAddressID { get; set; }
		public virtual string FirstName { get; set; }
		public virtual string InternationalName { get; set; }
		public virtual string InternationalStateAndCity { get; set; }
		public virtual string InternationalStreet { get; set; }
		public virtual string LastName { get; set; }
		public virtual string Name { get; set; }
		public virtual string Phone { get; set; }
		public virtual string Phone2 { get; set; }
		public virtual string Phone2AreaOrCityCode { get; set; }
		public virtual string Phone2CountryCode { get; set; }
		public virtual string Phone2CountryPrefix { get; set; }
		public virtual string Phone2LocalNumber { get; set; }
		public virtual string PhoneAreaOrCityCode { get; set; }
		public virtual string PhoneCountryCode { get; set; }
		public virtual string PhoneCountryCodePrefix { get; set; }
		public virtual string PhoneLocalNumber { get; set; }
		public virtual string PostalCode { get; set; }
		public virtual string StateOrProvince { get; set; }
		public virtual string Street { get; set; }
		public virtual string Street1 { get; set; }
		public virtual string Street2 { get; set; }
	}
}