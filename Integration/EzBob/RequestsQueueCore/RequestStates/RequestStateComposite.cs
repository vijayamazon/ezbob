using System;
using System.Collections.Generic;
using System.Linq;

namespace EzBob.RequestsQueueCore.RequestStates
{
	internal class RequestStateComposite : RequestStateBase
	{
		public static IRequestState Create( IEnumerable<IRequestState> statesList )
		{
			return new RequestStateComposite( statesList );
		}

		private readonly bool _InProgress;
		private readonly RequestErorrInfo _ErrorInfo;
		private readonly RequestStateEnum _State;
		private readonly bool _HasError;
		private readonly bool _IsDone;
		private readonly bool _IsSuccess;
		private readonly bool _IsCanceled;
		private readonly bool _NotFound;

		private RequestStateComposite(IEnumerable<IRequestState> data)
		{
			var statesList = data.ToList();
			
			if ( statesList.Any( s => s.HasError() ) )
			{
				_ErrorInfo = new RequestErorrInfo( statesList.Where( s => s.HasError() ).SelectMany( s => s.ErorrInfo.CompositeException.Exceptions ) );
			}
			var states = statesList.Select( s => s.State );

			_IsDone = states.All( IsDone );
			_IsSuccess = states.All( IsSuccess );

			_InProgress = states.Any( InProgress );
			_HasError = states.Any( s => HasError(s) && _IsDone );
			_IsCanceled = states.Any( s => IsCanceled(s) && _IsDone );

			var isNew = states.All( IsNew );
			_NotFound = states.All( NotFound );

			if ( isNew )
			{
				_State = RequestStateEnum.New;
			}
			else if ( _NotFound )
			{
				_State = RequestStateEnum.NotFound;
			}
			else if ( _IsCanceled )
			{
				_State = RequestStateEnum.Canceled;
			}
			else if ( _InProgress )
			{
				_State = RequestStateEnum.Processing;
			}
			else if ( _IsSuccess )
			{
				_State = RequestStateEnum.Success;
			}
			else if ( _HasError )
			{
				_State = RequestStateEnum.Error;
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public override RequestStateEnum State
		{
			get 
			{
				return _State;
			}
		}

		public override RequestErorrInfo ErorrInfo
		{
			get { return _ErrorInfo; }
		}

		public override bool InProgress()
		{
			return _InProgress;
		}

		public override bool HasError()
		{
			return _HasError;
		}

		public override bool IsDone()
		{
			return _IsDone;
		}

		public override bool IsCanceled()
		{
			return _IsCanceled;
		}

		public override bool IsSuccess()
		{
			return _IsSuccess;
		}

		public override bool IsNotFound()
		{
			return _NotFound;
		}
	}
}