using System;
using System.Collections.Concurrent;
using System.Linq;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.RequestsQueueCore.Handle;
using EzBob.RequestsQueueCore.RequestInfos;
using EzBob.RequestsQueueCore.RequestStates;
using EzBob.RequestsQueueCore.RequestTasks;
using EzBob.RequestsQueueCore.RequestTasks.States;
using EzBob.RequestsQueueCore.TaskExecutors;

namespace EzBob.RequestsQueueCore
{
	public class ExternalRequestsManager
	{
		private readonly ConcurrentDictionary<IRequestHandle, IRequestTask> _Requests = new ConcurrentDictionary<IRequestHandle, IRequestTask>();
		private readonly TaskExecutor _TaskExecutor;

		public ExternalRequestsManager( IMarketplaceType marketplace )
		{
			_TaskExecutor = new TaskExecutor( marketplace );
		}

		public IRequestHandle CreateRequest( IRequestData requestData )
		{
			var req = CreateNewRequest( requestData );

			var task = CreateTask( requestData );

			_Requests.TryAdd( req, task );

			return req;
		}

		public IRequestState ExecuteRequest( IRequestHandle handle )
		{
			IRequestTask task;
			if (! _Requests.TryGetValue( handle, out task ) )
			{
				return RequestStateSingle.Create( RequestTaskState.NotFound );
			}

			task.AddToQueue();

			return task.TaskState;
		}

		public IRequestState GetState( IRequestHandle handle )
		{
			IRequestTask task;
			if ( !_Requests.TryGetValue( handle, out task ) )
			{
				return RequestStateSingle.Create( RequestTaskState.NotFound );
			}

			return task.TaskState;

		}

		public IRequestState CancelRequest( IRequestHandle handle )
		{
			IRequestTask task;
			if ( !_Requests.TryGetValue( handle, out task ) )
			{

				return RequestStateSingle.Create( RequestTaskState.NotFound );

			}

			task.Cancel();

			return task.TaskState;
		}

		public void Exit()
		{
			_Requests.Values.ToList().ForEach( t => t.Cancel() );

			_TaskExecutor.Exit();

			_Requests.Clear();
		}

		private IRequestTask CreateTask( IRequestData requestData )
		{
			return new RequestTask( _TaskExecutor, requestData );
		}

		private IRequestHandle CreateNewRequest( IRequestData requestData )
		{
			return RequestHandle.Create( requestData );
		}

		
	}
}