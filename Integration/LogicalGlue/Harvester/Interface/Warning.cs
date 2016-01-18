namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using Newtonsoft.Json;

	public class Warning {
		[JsonProperty(PropertyName = "currentValue")]
		public string Value { get; set; }

		[JsonProperty(PropertyName = "featureName")]
		public string FeatureName { get; set; }

		[JsonProperty(PropertyName = "minValue")]
		public string MinValue { get; set; }

		[JsonProperty(PropertyName = "maxValue")]
		public string MaxValue { get; set; }
	} // class Warning
} // namespace
