namespace EzBobRest.NancySwagger.Model
{
    using Newtonsoft.Json;
    using NJsonSchema;

    public class SwaggerRequestParameter
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("in")]
        public string In { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("schema")]
        public JsonSchema4 Schema { get; set; }
    }
}