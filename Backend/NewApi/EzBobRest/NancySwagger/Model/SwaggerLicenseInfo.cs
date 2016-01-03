namespace EzBobRest.NancySwagger.Model
{
    using Newtonsoft.Json;

    public class SwaggerLicenseInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}