using EzBob.RequestsQueueCore.RequestTasks;

namespace EzBob.RequestsQueueCore.TaskExecutors
{
	internal interface ITaskExecutor
	{
		void AddToQueue( IExecutionTask task );
	}
}