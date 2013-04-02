using System;
using System.Threading;
using System.Threading.Tasks;
using EzBob.RequestsQueueCore.Handle;
using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestInfos
{
	public interface IRequestData
	{
		string RequestName { get; }

		IRequestHandle CreateAndExecuteRequest( RequestsManagerStorage storageManagers );

		IRequestHandle CreateRequest( RequestsManagerStorage storageManagers );
		IRequestState GetRequestState( RequestsManagerStorage storageManagers );
		IRequestState ExecuteRequest( RequestsManagerStorage storageManagers );
		IRequestState CancelRequest( RequestsManagerStorage storageManagers );
		
		Task CreateTask(CancellationToken cancelationToken);
	}
}