using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestTasks.States
{
	internal class RequestTaskStateNew : RequestTaskState
	{
		public override RequestStateEnum TaskState
		{
			get { return RequestStateEnum.New; }
		}

		public override void AddToQueue( RequestTask context )
		{
			context.ChangeState( InQueue );
			context.InternalAddToQueue();
		}

		public override void Cancel( RequestTask context )
		{
			context.ChangeState( Canceled );			
		}
	}
}