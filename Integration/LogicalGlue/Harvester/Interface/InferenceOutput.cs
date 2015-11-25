namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using Newtonsoft.Json;

	public class InferenceOutput {
		[JsonProperty(PropertyName = "FL", NullValueHandling = NullValueHandling.Ignore)]
		public ModelOutput FuzzyLogic { get; set; }

		[JsonProperty(PropertyName = "NN", NullValueHandling = NullValueHandling.Ignore)]
		public ModelOutput NeuralNetwork { get; set; }

		[JsonProperty(PropertyName = "bucket", NullValueHandling = NullValueHandling.Ignore)]
		public Bucket Bucket { get; set; }
	} // class InferenceOutput
} // namespace
