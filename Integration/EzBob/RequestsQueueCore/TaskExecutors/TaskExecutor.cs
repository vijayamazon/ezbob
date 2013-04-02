using System.Collections.Concurrent;
using System.Threading;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.RequestsQueueCore.RequestTasks;

namespace EzBob.RequestsQueueCore.TaskExecutors
{
	internal class TaskExecutor : ITaskExecutor
	{
		private readonly Thread _Thread;

		private readonly EventWaitHandle _Finish = new ManualResetEvent( false );
		private readonly EventWaitHandle _Finished = new ManualResetEvent( false );

		private readonly EventWaitHandle _New = new ManualResetEvent( false );
		
		//private readonly EventWaitHandle _Finish = new ManualResetEvent( false );
		private readonly ConcurrentQueue<IExecutionTask> _Queue = new ConcurrentQueue<IExecutionTask>();

		public TaskExecutor( IDatabaseMarketplace marketplace )
		{
			_Thread = new Thread( MainCicle )
			          	{
							Name = marketplace.DisplayName
			          	};
			_Thread.Start();
		}

		public void AddToQueue( IExecutionTask task )
		{
			_Queue.Enqueue( task );
			_New.Set();
		}

		public void Exit()
		{
			_Finish.Set();
			_Finish.WaitOne();
		}

		private void MainCicle()
		{
			var handles = new[] { _Finish, _New };

			do
			{
				if ( WaitHandle.WaitAny( handles ) == 1 )
				{
					_New.Reset();

					ProcessCicle();
				}

			} while ( !_Finish.WaitOne( 100 ) );

			_Finished.Set();
		}

		private void ProcessCicle()
		{
			while ( !_Queue.IsEmpty )
			{
				IExecutionTask task;
				if ( _Queue.TryDequeue( out task ) )
				{
					task.Execute();
				}
			}
		}
	}
}