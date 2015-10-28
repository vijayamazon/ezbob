namespace Ezbob.Integration.LogicalGlue.Processor {
	using System;
	using Ezbob.Integration.LogicalGlue.Interface;

	public class Processor : IProcessor {
		public Inference EvaluateCustomer(int customerID) {
			// 1. call injected IHarvester.
			// 2. save its Reply.
			// 3. convert Reply to Inference.
			return new Inference(); // TODO
		} // EvaluateCustomer

		public Inference GetHistoricalEvaluation(int customerID, DateTime date) {
			// 1. Load Inference from DB.
			return new Inference(); // TODO
		} // GetHistoricalEvaluation
	} // class Processor
} // namespace
