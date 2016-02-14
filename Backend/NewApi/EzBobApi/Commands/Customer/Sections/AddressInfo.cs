namespace EzBobApi.Commands.Customer.Sections
{
    using EzBobModels.Enums;

    public class AddressInfo {
        public CustomerAddressType? addressType { get; set; }//not begins from upper case because in DB it's not upper case.

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
    }
}
