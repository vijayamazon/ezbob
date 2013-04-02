using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EzBob.RequestsQueueCore.RequestInfos;
using EzBob.RequestsQueueCore.RequestStates;
using EzBob.RequestsQueueCore.RequestTasks.States;
using EzBob.RequestsQueueCore.TaskExecutors;

namespace EzBob.RequestsQueueCore.RequestTasks
{
	internal sealed class  RequestTask : StateOriented<IRequestTaskState>, IRequestTask
	{
		private readonly ITaskExecutor _TastExecutor;
		private readonly IRequestData _RequestData;

		private readonly CancellationTokenSource _TaskTokenSource;

		private readonly Task _TaskWork;
		private Task _TaskWait;

		public RequestTask( ITaskExecutor tastExecutor, IRequestData requestData )
		{
			_TastExecutor = tastExecutor;
			_RequestData = requestData;

			_TaskTokenSource = new CancellationTokenSource();

			ChangeState( RequestTaskState.New );

			_TaskWork = _RequestData.CreateTask( _TaskTokenSource.Token );			
			_TaskWait = Task.Factory.StartNew( WaitWorkingTaskForEnd );
		}

		private void WaitWorkingTaskForEnd()
		{
			try
			{
				_TaskWork.Wait();

				if ( _TaskWork.IsCanceled )
				{
					ChangeState( RequestTaskState.Canceled );
					return;
				}

				ChangeState( RequestTaskState.Success );

			}
			catch ( AggregateException ex )
			{
				if ( ex.InnerException is TaskCanceledException )
				{
					ChangeState( RequestTaskState.Canceled );
				}
				else
				{
					ChangeState(new RequestTaskStateError(ex.InnerException));
				}
			}
			catch ( OperationCanceledException )
			{
				ChangeState( RequestTaskState.Canceled );
			}
			catch ( Exception ex )
			{
				ChangeState(new RequestTaskStateError(ex));
			}			
		}

		public IRequestState TaskState
		{
			get { return RequestStateSingle.Create( State ); }
		}

		public void Cancel()
		{
			State.Cancel( this );
		}

		public void Execute()
		{
			State.Execute( this );
		}

		public override void ChangeState( IRequestTaskState newState )
		{
			base.ChangeState( newState );
			Debug.WriteLine( "[{0}] {1} - {2}", DateTime.Now, _RequestData.RequestName, State );
		}

		public void AddToQueue()
		{
			State.AddToQueue( this );			
		}

		internal void InternalAddToQueue()
		{
			_TastExecutor.AddToQueue( this );
		}

		internal void InternalExecute()
		{
			try
			{
				
				_TaskWork.Start();
				/*var task = _RequestData.CreateTask( _TaskTokenSource.Token );				
				task.Start();
				task.Wait( _TaskTokenSource.Token );

				if ( task.IsCanceled )
				{
					ChangeState( RequestTaskState.Canceled );
				}*/
			}
			catch ( AggregateException ex )
			{
				ChangeState( new RequestTaskStateError( ex.InnerException ) );
				return;
			}
			catch ( OperationCanceledException )
			{
				ChangeState( RequestTaskState.Canceled );
				return;
			}
			catch ( Exception ex )
			{
				ChangeState( new RequestTaskStateError( ex ) );
				return;
			}

			if ( State.TaskState == RequestStateEnum.Canceled )
			{
				return;
			}
			else
			{
				//ChangeState(RequestTaskState.Success);
			}
		}

		internal void InternalCancelRequest()
		{			
			_TaskTokenSource.Cancel();
		}
	}
}