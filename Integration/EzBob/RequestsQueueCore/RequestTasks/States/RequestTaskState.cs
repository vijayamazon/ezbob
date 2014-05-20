namespace EzBob.RequestsQueueCore.RequestTasks.States
{
	using System;
	using System.Diagnostics;
	using CommonLib;
	using RequestStates;

	abstract class RequestTaskState : IRequestTaskState
	{
		public static readonly IRequestTaskState New = new RequestTaskStateNew();
		public static readonly IRequestTaskState InQueue = new RequestTaskStateInQueue();
		public static readonly IRequestTaskState Canceled = new RequestTaskStateCanceled();
		public static readonly IRequestTaskState Processing = new RequestTaskStateProcessing();
		public static readonly IRequestTaskState Success = new RequestTaskStateSuccess();
		public static readonly IRequestTaskState NotFound = new RequestTaskStateNotFound();
		public abstract RequestStateEnum TaskState { get; }
		
		public virtual void AddToQueue(RequestTask context)
		{
			WriteToLog( string.Format( "Operation AddToQueue Fail in State: {0}", this ) );
		}

		public virtual void Execute( RequestTask context )
		{
			WriteToLog( string.Format( "Operation Execute Fail in State: {0}", this ) );
		}

		public virtual void Cancel( RequestTask context )
		{
			WriteToLog( string.Format( "Operation Cancel Fail in State: {0}", this ) );
		}		

		public override string ToString()
		{
			return TaskState.ToString();
		}

		protected void WriteToLog( string message, WriteLogType messageType = WriteLogType.Debug, Exception ex = null )
		{
			WriteLoggerHelper.Write(message, messageType, null, ex);
			Debug.WriteLine( message );
		}
	}
}