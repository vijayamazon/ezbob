namespace Ezbob.Integration.LogicalGlue.Harvester {
	using Ezbob.Integration.LogicalGlue.HarvesterInterface;

	public class Harvester : IHarvester {
		public Response EvaluateCustomer(int customerID) {
			// 1. Send REST API request to Logical Glue endpoint.
			// 2. Load API call into Reply object.
			return new Response(""); // TODO
		} // EvaluateCustomer
	} // class Harvester
} // namespace
