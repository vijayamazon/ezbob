namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Strategies.LogicalGlue;
	using EzService.ActionResults.Investor;

	partial class EzServiceImplementation : IEzServiceLogicalGlue {
		public LogicalGlueResult LogicalGlueGetLastInference(int underwriterID, int customerID, DateTime? date, bool includeTryouts) {
			GetLatestKnownInference strategy;
			var metadata = ExecuteSync(out strategy, customerID, underwriterID, customerID, date, includeTryouts);
			var result = LogicalGlueResult.FromInference(strategy.Inference, customerID, Log, DB);
			return result;
		} // LoicalGlueGetLastInference

		public IList<LogicalGlueResult> LogicalGlueGetHistory(int underwriterID, int customerID) {
			GetHistoryInferences strategy;
			var metadata = ExecuteSync(out strategy, customerID, underwriterID, customerID);
			var result = strategy.Inferences.Select(x => LogicalGlueResult.FromInference(x, customerID, Log, DB)).ToList();
			return result;
		} // LogicalGlueGetHistory

		public LogicalGlueResult LogicalGlueGetTryout(int underwriterID, int customerID, decimal monthlyRepayment, bool isTryout) {
			GetTryoutInference strategy;
			var metadata = ExecuteSync(out strategy, customerID, underwriterID, customerID, monthlyRepayment, isTryout);
			var result = LogicalGlueResult.FromInference(strategy.Inference, customerID, Log, DB);
			return result;
		} // LogicalGlueGetTryout

		public BoolActionResult LogicalGlueSetAsCurrent(int underwriterID, int customerID, Guid uniqueID) {
			SetRequestIsTryOut instance;

			var amd = ExecuteSync(out instance, customerID, underwriterID, uniqueID, false);

			return new BoolActionResult {
				MetaData = amd,
				Value = instance.Success,
			};
		} // LogicalGlueSetAsCurrent
	}//EzServiceImplementation LogicalGlue
}//ns

