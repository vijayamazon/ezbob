namespace Ezbob.Integration.LogicalGlue.Driver {
	using System;
	using Ezbob.Integration.LogicalGlue.Interface;
	using Ezbob.Integration.LogicalGlue.Models;

	public class Driver : ILogicalGlue {
		public Driver() {
		} // constructor

		public IInferenceResult EvaluateCustomer(int customerID) {
			return new InferenceResult(); // TODO
		} // EvaluateCustomer

		public IInferenceResult GetHistoricalEvaluation(int customerID, DateTime date) {
			return new InferenceResult(); // TODO
		} // GetHistoricalEvaluation
	} // class Driver
} // namespace
