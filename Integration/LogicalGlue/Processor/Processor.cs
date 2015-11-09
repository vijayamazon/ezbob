namespace Ezbob.Integration.LogicalGlue.Processor {
	using System;
	using Ezbob.Integration.LogicalGlue.HarvesterInterface;
	using Ezbob.Integration.LogicalGlue.Interface;
	using Ezbob.Integration.LogicalGlue.KeeperInterface;
	using Ezbob.Logger;
	using log4net;

	public class Processor : IProcessor {
		public Processor(IKeeper keeper, IHarvester harvester, ILog log) {
			this.keeper = keeper;
			this.harvester = harvester;
			this.log = new SafeILog(log);
		} // constructor

		public Inference Infer(int customerID) {
			// 1. Collect customer data from DB (via injected DB interface).
			// 2. Load inference input data (via injected IHarvester) and save it (via injected DB interface).
			// 3. Load inference output data of fuzzy logic model (via injected IHarvester)
			//    and save it (via injected DB interface).
			// 4. Load inference output data of neural network model (via injected IHarvester)
			//    and save it (via injected DB interface).
			// 5. Convert model outputs to Inference.
			return new Inference(); // TODO
		} // Infer

		public Inference GetInference(int customerID) {
			return GetHistoricalInference(customerID, DateTime.UtcNow);
		} // GetInference

		public Inference GetHistoricalInference(int customerID, DateTime time) {
			// 1. Load Inference from DB.
			return new Inference(); // TODO
		} // GetHistoricalInference

		private readonly IKeeper keeper;
		private readonly IHarvester harvester;
		private readonly ASafeLog log;
	} // class Processor
} // namespace
