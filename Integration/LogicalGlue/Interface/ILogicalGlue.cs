namespace Ezbob.Integration.LogicalGlue.Interface {
	using System;

	public interface ILogicalGlue {
		IInference EvaluateCustomer(int customerID);
		IInference GetHistoricalEvaluation(int customerID, DateTime date);
	} // interface ILogicalGlue
} // namespace
