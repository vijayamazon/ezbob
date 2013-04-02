using System.Collections.Generic;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore
{
	internal interface IEzBobIntegrationWorkflows
	{
		int UpdateCustomerData( int customerId );
		int UpdateCustomerMarketPlaceData( int customerMarketPlaceId );
		bool IsRequestDone( int requestNumber );
		IRequestState GetRequestState( int requestNumber );
		string GetError( int requestNumber );
		void Exit();
		IAnalysisDataInfo GetAnalysisValuesByCustomerMarketPlace(int customerMarketPlaceId);
		IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int umi);
	}
}