namespace Ezbob.Integration.LogicalGlue.Harvester {
	using System;
	using Ezbob.Integration.LogicalGlue.Interface;
	using Ezbob.Integration.LogicalGlue.Models;

	public class Harvester : ILogicalGlue {
		public Harvester() {
		} // constructor

		public IInference EvaluateCustomer(int customerID) {
			return new Inference(); // TODO
		} // EvaluateCustomer

		public IInference GetHistoricalEvaluation(int customerID, DateTime date) {
			return new Inference(); // TODO
		} // GetHistoricalEvaluation
	} // class Harvester
} // namespace
