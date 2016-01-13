namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using Newtonsoft.Json;

	public class InferenceOutput {
		[JsonProperty(PropertyName = "decision", NullValueHandling = NullValueHandling.Ignore)]
		public InferenceOutputDecision Decision { get; set; }
	} // class InferenceOutput
} // namespace
