using System;
using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestTasks
{
	internal interface IRequestTask: IExecutionTask
	{
		void AddToQueue();
		IRequestState TaskState { get; }
		void Cancel();			
	}
}