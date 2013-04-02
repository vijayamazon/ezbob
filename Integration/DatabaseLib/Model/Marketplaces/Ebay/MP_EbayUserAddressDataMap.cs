using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EbayUserAddressDataMap : ClassMap<MP_EbayUserAddressData>
	{
		public MP_EbayUserAddressDataMap()
		{
			Table( "MP_EbayUserAddressData" );
			Id(x => x.Id);
			
			Map( x => x.AddressID );
			Map( x => x.AddressOwner );
			Map( x => x.AddressRecordType );
			Map( x => x.AddressStatus );
			Map( x => x.AddressUsage );
			Map( x => x.CityName );
			Map( x => x.CompanyName );
			Map( x => x.CountryCode );
			Map( x => x.CountryName );
			Map( x => x.County );
			Map( x => x.ExternalAddressID );
			Map( x => x.FirstName );
			Map( x => x.InternationalName );
			Map( x => x.InternationalStateAndCity );
			Map( x => x.InternationalStreet );
			Map( x => x.LastName );
			Map( x => x.Name );
			Map( x => x.Phone );
			Map( x => x.Phone2 );
			Map( x => x.Phone2AreaOrCityCode );
			Map( x => x.Phone2CountryCode );
			Map( x => x.Phone2CountryPrefix );
			Map( x => x.Phone2LocalNumber );
			Map( x => x.PhoneAreaOrCityCode );
			Map( x => x.PhoneCountryCode );
			Map( x => x.PhoneCountryCodePrefix );
			Map( x => x.PhoneLocalNumber );
			Map( x => x.PostalCode );
			Map( x => x.StateOrProvince );
			Map( x => x.Street );
			Map( x => x.Street1 );
			Map( x => x.Street2 );
		}
	}
}