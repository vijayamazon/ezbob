using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib
{
	public static class ConvertTypesHelper
	{
		public static DatabaseShipingAddress ConvertToDatabaseType( this AddressType address )
		{
			return new DatabaseShipingAddress
			{
				AddressID = address.AddressID,
				AddressOwner = address.AddressOwner.ToString(),
				AddressRecordType = address.AddressRecordType.ToString(),
				AddressStatus = address.AddressStatus.ToString(),
				AddressUsage = address.AddressUsage.ToString(),
				CityName = address.CityName,
				CompanyName = address.CompanyName,
				CountryCode = address.Country.ToString(),
				CountryName = address.CountryName,
				County = address.County,
				ExternalAddressID = address.ExternalAddressID,
				FirstName = address.FirstName,
				InternationalName = address.InternationalName,
				InternationalStateAndCity = address.InternationalStateAndCity,
				InternationalStreet = address.InternationalStreet,
				LastName = address.LastName,
				Name = address.Name,
				Phone = address.Phone,
				Phone2 = address.Phone2,
				Phone2AreaOrCityCode = address.Phone2AreaOrCityCode,
				Phone2CountryCode = address.Phone2CountryCode.ToString(),
				Phone2CountryPrefix = address.Phone2CountryPrefix,
				Phone2LocalNumber = address.Phone2LocalNumber,
				PhoneAreaOrCityCode = address.PhoneAreaOrCityCode,
				PhoneCountryCode = address.PhoneCountryCode.ToString(),
				PhoneCountryCodePrefix = address.PhoneCountryPrefix,
				PhoneLocalNumber = address.PhoneLocalNumber,
				PostalCode = address.PostalCode,
				StateOrProvince = address.StateOrProvince,
				Street = address.Street,
				Street1 = address.Street1,
				Street2 = address.Street2,
			};
		}

	}
}
