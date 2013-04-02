using System;
using System.Diagnostics;
using EzBob.RequestsQueueCore.RequestTasks.States;

namespace EzBob.RequestsQueueCore.RequestStates
{
	class RequestStateSingle : RequestStateBase
	{
		private readonly RequestStateEnum _State;
		private readonly RequestErorrInfo _ErorrInfo;

		public static RequestStateSingle Create( )
		{
			return RequestStateSingle.Create( RequestTaskState.New );
		}

		public static RequestStateSingle Create( IRequestTaskState state )
		{
			var s = state;
			if ( s.TaskState == RequestStateEnum.Error )
			{
				var stateError = s as RequestTaskStateError;
				Debug.Assert( stateError != null );

				return new RequestStateSingle(new RequestErorrInfo(stateError.Exception));
			}
			else
			{
				return new RequestStateSingle(s.TaskState );
			}
		}

		private RequestStateSingle(RequestStateEnum state = RequestStateEnum.New)
			: this( state, null )
		{
			
		}

		private RequestStateSingle( RequestErorrInfo erorrInfo )
			: this(RequestStateEnum.Error, erorrInfo )
		{
		}

		private RequestStateSingle( RequestStateEnum state, RequestErorrInfo erorrInfo )
		{
			_State = state;
			_ErorrInfo = erorrInfo;
		}

		public override RequestStateEnum State
		{
			get { return _State; }
		}

		public override RequestErorrInfo ErorrInfo
		{
			get { return _ErorrInfo; }
		}

		public override bool InProgress()
		{
			return InProgress( State );
		}

		public override bool HasError()
		{
			return HasError(State);
		}

		public override bool IsDone()
		{
			return IsDone( State );

		}

		public override bool IsCanceled()
		{ 
			return IsCanceled(State);
		}

		public override bool IsSuccess()
		{
			return IsSuccess( State );
		}

		public override bool IsNotFound()
		{
			return NotFound( State );
		}

		public override string ToString()
		{
			return State.ToString();
		}
	}
}