namespace Ezbob.Integration.LogicalGlue.Interface {
	using System;

	public interface ILogicalGlue {
		IInferenceResult EvaluateCustomer(int customerID);
		IInferenceResult GetHistoricalEvaluation(int customerID, DateTime date);
	} // interface ILogicalGlue
} // namespace
