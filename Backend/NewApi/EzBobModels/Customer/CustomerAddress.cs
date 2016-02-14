namespace EzBobModels.Customer
{
    using System;
    using System.Linq;
    using EzBobModels.Enums;

    [Serializable]
    public class CustomerAddress
    {
        public int? CustomerId { get; set; }
        public int? DirectorId { get; set; }
        public int? CompanyId { get; set; }
        public int? addressId { get; set; }//not begins from upper case because in DB it's not upper case.
        public CustomerAddressType? addressType { get; set; }//not begins from upper case because in DB it's not upper case.

        public string id { get; set; } //not begins from upper case because in DB it's not upper case.
        public string Organisation { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public string Rawpostcode { get; set; }
        public string Deliverypointsuffix { get; set; }
        public string Nohouseholds { get; set; }
        public string Smallorg { get; set; }
        public string Pobox { get; set; }
        public string Mailsortcode { get; set; }
        public string Udprn { get; set; }
        public bool? IsOwnerAccordingToLandRegistry { get; set; }

        public string FormattedAddress
        {
            get
            {
                return string.IsNullOrEmpty(Postcode)
                    ? null
                    : string.Join(
                        " ",
                        (new[] { Line1, Line2, Line3, Town, Country, Postcode })
                            .Select(x => (x ?? string.Empty).Trim())
                            .Where(x => !string.IsNullOrWhiteSpace(x)
                    )
                );
            }
        }

        public string[] AddressArray()
        {
            return new string[] { Line1, Line2, Line3, Town, Postcode };
        }
    }
}
