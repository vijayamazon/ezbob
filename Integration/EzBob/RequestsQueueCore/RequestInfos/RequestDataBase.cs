using System;
using System.Threading;
using System.Threading.Tasks;
using EzBob.RequestsQueueCore.Handle;
using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestInfos
{
	internal abstract class RequestDataBase : IRequestData
	{
		protected RequestDataBase(string name)
		{
			RequestName = name;
		}

		public string RequestName { get; private set; }

		protected IRequestHandle Handle { get; private set; }

		protected abstract IRequestHandle CreateRequestInternal( RequestsManagerStorage storageManagers );
		
		public abstract IRequestState ExecuteRequest(RequestsManagerStorage storageManagers);

		public IRequestHandle CreateRequest(RequestsManagerStorage storageManagers)
		{
			return Handle ?? ( Handle = CreateRequestInternal( storageManagers ) );
		}

		public IRequestHandle CreateAndExecuteRequest(RequestsManagerStorage storageManagers)
		{
			var handle = CreateRequest( storageManagers );
			ExecuteRequest( storageManagers );
			return handle;
		}

		public abstract IRequestState GetRequestState(RequestsManagerStorage storageManagers);
		public abstract IRequestState CancelRequest(RequestsManagerStorage storageManagers);

		public abstract Task CreateTask(CancellationToken cancelationToken);
	}
}