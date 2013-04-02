using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestTasks.States
{
	internal class RequestTaskStateNotFound : RequestTaskState
	{
		public override RequestStateEnum TaskState
		{
			get { return RequestStateEnum.NotFound; }
		}
	}
}