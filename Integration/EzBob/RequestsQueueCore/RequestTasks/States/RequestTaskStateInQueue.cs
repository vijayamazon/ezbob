using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestTasks.States
{
	internal class RequestTaskStateInQueue : RequestTaskState
	{
		public override RequestStateEnum TaskState
		{
			get { return RequestStateEnum.InQueue; }
		}

		public override void Execute( RequestTask context )
		{
			context.ChangeState( Processing );
			context.InternalExecute();
		}

		public override void Cancel( RequestTask context )
		{
			context.ChangeState( Canceled );			
		}
	}
}