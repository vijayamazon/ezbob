using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EzBob.RequestsQueueCore.Handle;
using EzBob.RequestsQueueCore.RequestInfos;
using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore
{
	class ExternalRequestsController : IExternalRequestsController
	{
		private readonly RequestsManagerStorage _StorageManagers = new RequestsManagerStorage();

		public IRequestHandle CreateAndExecuteRequest( IRequestData data )
		{
			return data.CreateAndExecuteRequest( _StorageManagers );			
		}

		public IRequestHandle CreateRequest( IRequestData data )
		{
			return data.CreateRequest( _StorageManagers );
		}

		public IRequestState ExecuteRequest( IRequestHandle handle )
		{
			return handle.RequestData.ExecuteRequest( _StorageManagers );
		}

		public IRequestState GetRequestState(IRequestHandle handle)
		{
			var requestState = handle.RequestData.GetRequestState( _StorageManagers );
			return requestState;
		}

		public IRequestState CancelRequest(IRequestHandle handle)
		{
			return handle.RequestData.CancelRequest( _StorageManagers );
		}

		public IRequestState ProcessRequest( IRequestData data )
		{
			var task = Task.Factory.StartNew( () =>
			    {
			        var requestHandle = CreateAndExecuteRequest( data );

					IRequestState state = RequestStateSingle.Create();

			        while ( !state.IsDone() )
			        {
			            state = GetRequestState( requestHandle );
			        }

			        return state;
			    } );

			return task.Result;
		}
		
		public void Exit()
		{
			_StorageManagers.Exit();			
		}
		
		public IRequestState WaitOne( IRequestHandle requestHandle )
		{
			IRequestState state = RequestStateSingle.Create();

			while ( !state.IsDone() )
			{
				state = GetRequestState( requestHandle );
			}

			return state;
		}

		public IEnumerable<IRequestState> WaitAll( IRequestHandle[] requestHandles )
		{
			while ( !requestHandles.All( h => GetRequestState( h ).IsDone() ) )
			{
			}

			return requestHandles.Select( GetRequestState ).ToArray();
		}
	}
}