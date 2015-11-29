namespace Ezbob.Integration.LogicalGlue.Harvester.Implementation {
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;

	public class Harvester : IHarvester {
		public Harvester(ASafeLog log) {
			this.log = log.Safe();
		} // constructor

		public Response<Reply> Infer(InferenceInput inputData) {
			// 1. Send REST API request to Logical Glue endpoint.
			// 2. Load API call into InferenceOutput object.
			return new Response<Reply>(""); // TODO
		} // Infer

		private readonly ASafeLog log;
	} // class Harvester
} // namespace
