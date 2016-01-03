using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.EBay {
    /// <summary>
    /// DTO for MP_EbayUserAddressData
    /// </summary>
    public class EbayUserAddressData {
        public int Id { get; set; }
        public string AddressId { get; set; }
        public string AddressOwner { get; set; }
        public string AddressRecordType { get; set; }
        public string AddressStatus { get; set; }
        public string AddressUsage { get; set; }
        public string CityName { get; set; }
        public string CompanyName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string County { get; set; }
        public string ExternalAddressID { get; set; }
        public string FirstName { get; set; }
        public string InternationalName { get; set; }
        public string InternationalStateAndCity { get; set; }
        public string InternationalStreet { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Phone2 { get; set; }
        public string Phone2AreaOrCityCode { get; set; }
        public string Phone2CountryCode { get; set; }
        public string Phone2CountryPrefix { get; set; }
        public string Phone2LocalNumber { get; set; }
        public string PhoneAreaOrCityCode { get; set; }
        public string PhoneCountryCode { get; set; }
        public string PhoneCountryCodePrefix { get; set; }
        public string PhoneLocalNumber { get; set; }
        public string PostalCode { get; set; }
        public string StateOrProvince { get; set; }
        public string Street { get; set; }
        public string Street1 { get; set; }
        public string Street2 { get; set; }
    }
}
