namespace EzBobModels.Postcode {
    public class PostcodesAddress {
        public string postcode { get; set; }
        public int quality { get; set; }
        public long eastings { get; set; }
        public long northings { get; set; }
        public string country { get; set; }
        public string nhs_ha { get; set; }
        public decimal longitude { get; set; }
        public decimal latitude { get; set; }
        public string parliamentary_constituency { get; set; }
        public string european_electoral_region { get; set; }
        public string primary_care_trust { get; set; }
        public string region { get; set; }
        public string lsoa { get; set; }
        public string msoa { get; set; }
        public string incode { get; set; }
        public string outcode { get; set; }
        public string admin_district { get; set; }
        public string parish { get; set; }
        public string admin_county { get; set; }
        public string admin_ward { get; set; }
        public string ccg { get; set; }
        public string nuts { get; set; }
        public PostCodeResultCodes codes { get; set; }
    }
}
