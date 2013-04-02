using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestTasks.States
{
	internal class RequestTaskStateProcessing : RequestTaskState
	{
		public override RequestStateEnum TaskState
		{
			get { return RequestStateEnum.Processing; }
		}

		public override void Cancel( RequestTask context )
		{
			context.InternalCancelRequest();
		}
	}
}