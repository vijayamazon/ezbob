namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Newtonsoft.Json;

	public class InferenceOutputDecisionModels {
		[JsonProperty(PropertyName = "FL_response", NullValueHandling = NullValueHandling.Ignore)]
		public string FuzzyLogicResponse { get; set; }

		[JsonProperty(PropertyName = "NN_response", NullValueHandling = NullValueHandling.Ignore)]
		public string NeuralNetworkResponse { get; set; }

		[JsonProperty(PropertyName = "LR_response", NullValueHandling = NullValueHandling.Ignore)]
		public string LogisticRegressionResponse { get; set; }

		[JsonIgnore]
		public string this[ModelNames name] {
			get {
				switch (name) {
				case ModelNames.FuzzyLogic:
					return FuzzyLogicResponse;

				case ModelNames.NeuralNetwork:
					return NeuralNetworkResponse;

				case ModelNames.LogisticRegression:
					return LogisticRegressionResponse;

				default:
					throw new ArgumentOutOfRangeException("name");
				} // switch
			} // get
		} // indexer
	} // class InferenceOutputDecisionModels
} // namespace
