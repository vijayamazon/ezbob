namespace Ezbob.Integration.LogicalGlue.Harvester {
	using Ezbob.Integration.LogicalGlue.HarvesterInterface;

	public class RestHarvester : IHarvester {
		public Response<InferenceInput> GetInferenceInputData(EvaluateeDetails evaluateeDetails) {
			// 1. Send REST API request to Logical Glue endpoint.
			// 2. Load API call into InferenceInput object.
			return new Response<InferenceInput>(""); // TODO
		} // GetInferenceInputData

		public Response<InferenceOutput> Infer(InferenceInput inputData) {
			// 1. Send REST API request to Logical Glue endpoint.
			// 2. Load API call into InferenceOutput object.
			return new Response<InferenceOutput>(""); // TODO
		} // Infer
	} // class RestHarvester
} // namespace
