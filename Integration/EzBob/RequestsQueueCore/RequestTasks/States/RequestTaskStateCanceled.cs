using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestTasks.States
{
	internal class RequestTaskStateCanceled : RequestTaskState
	{
		public override RequestStateEnum TaskState
		{
			get { return RequestStateEnum.Canceled; }
		}
	}
}