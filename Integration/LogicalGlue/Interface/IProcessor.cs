namespace Ezbob.Integration.LogicalGlue.Interface {
	using System;

	public interface IProcessor {
		Inference EvaluateCustomer(int customerID);
		Inference GetHistoricalEvaluation(int customerID, DateTime date);
	} // interface IProcessor
} // namespace
