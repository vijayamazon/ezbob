using System;
using EZBob.DatabaseLib;
using EzBob.CommonLib;
using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestTasks.States
{
	internal class RequestTaskStateError : RequestTaskState
	{
		public RequestTaskStateError(Exception exception)
		{
			Exception = exception;
			
			WriteToLog( Exception.Message, WriteLogType.Error, exception );
		}

		public Exception Exception { get; private set; }

		public override RequestStateEnum TaskState
		{
			get { return RequestStateEnum.Error; }
		}
		
	}
}