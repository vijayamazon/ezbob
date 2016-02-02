namespace EzBobModels.SimplyPostcode {
    /// <summary>
    /// SimplyPostcode detailed address
    /// </summary>
    public class SimplyPostcodeDatailedAddress {
        public int Found { get; set; }
        public string Errormessage { get; set; }
        public string Id { get; set; }
        public string Organization { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public string RawPostcode { get; set; }
        public string DeliveryPointSiffix { get; set; }
        public int NoHouseholds { get; set; }
        public string SmallOrg { get; set; }
        public string Pobox { get; set; }
        public string MailSortCode { get; set; }
        public string Udprn { get; set; }
    }
}
