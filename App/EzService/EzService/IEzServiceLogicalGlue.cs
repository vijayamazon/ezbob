namespace EzService {
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using EzService.ActionResults.Investor;

	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IEzServiceLogicalGlue {
		[OperationContract]
		LogicalGlueResult LogicalGlueGetLastInference(int underwriterID, int customerID, DateTime? date, bool includeTryouts);

		[OperationContract]
		IList<LogicalGlueResult> LogicalGlueGetHistory(int underwriterID, int customerID);

		[OperationContract]
		LogicalGlueResult LogicalGlueGetTryout(int underwriterID, int customerID, decimal monthlyRepayment, bool isTryout);

		[OperationContract]
		BoolActionResult LogicalGlueSetAsCurrent(int underwriterID, int customerID, Guid uniqueID);

		[OperationContract]
		ActionMetaData BackfillLogicalGlueForAll();
	} // interface IEzServiceLogicalGlue
} // namespace  
