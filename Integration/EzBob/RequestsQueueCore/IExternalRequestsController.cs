using System;
using EzBob.RequestsQueueCore.Handle;
using EzBob.RequestsQueueCore.RequestInfos;
using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore
{
	interface IExternalRequestsController
	{
		IRequestHandle CreateAndExecuteRequest( IRequestData data );

		IRequestState GetRequestState( IRequestHandle handle );

		IRequestState CancelRequest( IRequestHandle handle );

		/// <summary>
		/// Synchronous execution of the Procedure
		/// </summary>
		/// <param name="a">action</param>
		/// <returns>Request state</returns>
		IRequestState ProcessRequest( IRequestData data );
	}
}