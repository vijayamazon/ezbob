using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestTasks.States
{
	internal class RequestTaskStateSuccess : RequestTaskState
	{
		public override RequestStateEnum TaskState
		{
			get { return RequestStateEnum.Success; }
		}
	}
}