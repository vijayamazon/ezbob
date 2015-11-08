namespace Ezbob.Integration.LogicalGlue.Processor {
	using System;
	using Ezbob.Integration.LogicalGlue.HarvesterInterface;
	using Ezbob.Integration.LogicalGlue.Interface;
	using Ezbob.Integration.LogicalGlue.KeeperInterface;
	using Ezbob.Logger;
	using log4net;

	public class Processor : IProcessor {
		public Processor() {
			Inject();
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

		[Injected]
		public IKeeper Keeper { get; set; }

		[Injected]
		public IHarvester Harvester { get; set; }

		[Injected]
		public ILog LogWriter { get; set; }

		public ASafeLog Log {
			get {
				if (this.log == null)
					this.log = new SafeILog(LogWriter);

				return this.log;
			} // get
		} // Log

		/// <summary>
		/// This is a temporary method that emulates injection.
		/// Once real injection is in place this method should be removed.
		/// </summary>
		private void Inject() {
			LogWriter = InjectorStub.GetLog();
			Keeper = InjectorStub.GetKeeper();
			Harvester = InjectorStub.GetHarvester();
		} // Inject
		private ASafeLog log;
	} // class Processor
} // namespace
