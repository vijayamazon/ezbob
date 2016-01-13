namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using Newtonsoft.Json;

	public class InferenceOutputDecisionModels {
		[JsonProperty(PropertyName = "FL_response", NullValueHandling = NullValueHandling.Ignore)]
		public string FuzzyLogicResponse { get; set; }

		[JsonProperty(PropertyName = "NN_response", NullValueHandling = NullValueHandling.Ignore)]
		public string NeuralNetworkResponse { get; set; }
	} // class InferenceOutputDecisionModels
} // namespace
