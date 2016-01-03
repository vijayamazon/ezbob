namespace EzBobRest.NancySwagger.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class SwaggerTypeDefinition
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("required")]
        public List<string> RequiredProperties { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, SwaggerTypeDefinition> Properties { get; set; }
    }
}