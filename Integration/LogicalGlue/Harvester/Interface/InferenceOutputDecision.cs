namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using Newtonsoft.Json;

	public class InferenceOutputDecision {
		[JsonProperty(PropertyName = "outcome", NullValueHandling = NullValueHandling.Ignore)]
		public string Outcome { get; set; }

		[JsonProperty(PropertyName = "reason", NullValueHandling = NullValueHandling.Ignore)]
		public string Reason { get; set; }

		[JsonProperty(PropertyName = "bucket", NullValueHandling = NullValueHandling.Ignore)]
		public string Bucket { get; set; }

		[JsonProperty(PropertyName = "models", NullValueHandling = NullValueHandling.Ignore)]
		public InferenceOutputDecisionModels Models { get; set; }
	} // class InferenceOutputDecision
} // namespace
