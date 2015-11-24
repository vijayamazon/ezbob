namespace Ezbob.Integration.LogicalGlue.Harvester.Implementation {
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Logger;
	using log4net;

	public class Harvester : IHarvester {
		public Harvester(ILog log) {
			this.log = new SafeILog(log);
		} // constructor

		public Response<InferenceOutput> Infer(InferenceInput inputData) {
			// 1. Send REST API request to Logical Glue endpoint.
			// 2. Load API call into InferenceOutput object.
			return new Response<InferenceOutput>(""); // TODO
		} // Infer

		private readonly ASafeLog log;
	} // class Harvester
} // namespace
