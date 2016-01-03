namespace EzBobRest.NancySwagger.Model
{
    using Newtonsoft.Json;

    public class SwaggerApiInfo
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("contact")]
        public SwaggerContactInfo ContactInfo { get; set; }

        [JsonProperty("license")]
        public SwaggerLicenseInfo LicenseInfo { get; set; }
    }
}