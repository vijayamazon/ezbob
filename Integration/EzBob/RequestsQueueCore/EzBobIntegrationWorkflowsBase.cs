using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EzBob.RequestsQueueCore.Handle;
using EzBob.RequestsQueueCore.RequestInfos;
using EzBob.RequestsQueueCore.RequestStates;
using EzBob.RequestsQueueCore.RequestTasks.States;

namespace EzBob.RequestsQueueCore
{
	abstract class EzBobIntegrationWorkflowsBase : IEzBobIntegrationWorkflows
	{
		private readonly ExternalRequestsController _Controller;
		private readonly ConcurrentDictionary<int, IRequestHandle> _Handles = new ConcurrentDictionary<int, IRequestHandle>();

		private int RequestsCounter;
		
		protected EzBobIntegrationWorkflowsBase()
		{
			_Controller = new ExternalRequestsController();
		}

		public abstract int UpdateCustomerData(int customerId);
		public abstract int UpdateCustomerMarketPlaceData(int customerMarketPlaceId);
		public abstract IAnalysisDataInfo GetAnalysisValuesByCustomerMarketPlace(int customerMarketPlaceId);

		public abstract IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int umi);			

		protected int CreateRequest( IRequestData requestinfo )
		{
			IRequestHandle h = _Controller.CreateAndExecuteRequest( requestinfo );

			int requestNumber = Interlocked.Increment( ref RequestsCounter );

			var rez = _Handles.TryAdd( requestNumber, h );
			Debug.Assert( rez );

			return requestNumber;
		}

		public bool IsRequestDone( int requestNumber )
		{
			IRequestHandle handle;

			if ( !_Handles.TryGetValue( requestNumber, out handle ) )
			{
				return true;
			}

			IRequestState requestState = _Controller.GetRequestState( handle );

			return requestState.IsDone();
		}

		public IRequestState GetRequestState( int requestNumber )
		{
			return GetRequestStateInternal( requestNumber );
		}


		public string GetError( int requestNumber )
		{
			IRequestState state = GetRequestStateInternal( requestNumber );

			return state.ErorrInfo.Message;
		}

		public virtual void Exit()
		{
			_Controller.Exit();
		}

		

		private IRequestState GetRequestStateInternal( int requestNumber )
		{
			IRequestHandle handle;

			IRequestState state = _Handles.TryGetValue( requestNumber, out handle )
			                      	? _Controller.GetRequestState( handle )
			                      	: RequestStateFactory.Create( RequestTaskState.NotFound );

			return state;
		}		
	}
}