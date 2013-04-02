using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestTasks.States
{
	internal interface IRequestTaskState
	{
		RequestStateEnum TaskState { get; }
		void AddToQueue(RequestTask context );
		void Execute(RequestTask context);
		void Cancel(RequestTask context);
	}
}