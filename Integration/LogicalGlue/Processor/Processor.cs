namespace Ezbob.Integration.LogicalGlue.Processor {
	using System;
	using Ezbob.Integration.LogicalGlue.Interface;

	public class Processor : IProcessor {
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

		public Inference GetHistoricalInference(int customerID, DateTime date) {
			// 1. Load Inference from DB.
			return new Inference(); // TODO
		} // GetHistoricalInference
	} // class Processor
} // namespace
