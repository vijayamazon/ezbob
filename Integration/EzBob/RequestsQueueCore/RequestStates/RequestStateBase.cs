namespace EzBob.RequestsQueueCore.RequestStates
{
	internal abstract class RequestStateBase : IRequestState
	{
		public abstract RequestStateEnum State { get; }
		public abstract RequestErorrInfo ErorrInfo { get; }
		public abstract bool InProgress();
		public abstract bool HasError();
		public abstract bool IsDone();
		public abstract bool IsCanceled();
		public abstract bool IsSuccess();
		public abstract bool IsNotFound();

		protected static bool IsNew( RequestStateEnum state )
		{
			return state == RequestStateEnum.New;
		}

		protected static bool InProgress( RequestStateEnum state )
		{
			return IsNew(state) ||
				state == RequestStateEnum.InQueue ||
				state == RequestStateEnum.Processing;
		}

		protected static bool IsDone( RequestStateEnum state )
		{
			return NotFound( state ) ||
					IsSuccess( state ) ||
					HasError( state ) ||
					IsCanceled( state );
		}

		protected static bool HasError( RequestStateEnum state )
		{
			return state == RequestStateEnum.Error;
		}

		protected static bool IsCanceled( RequestStateEnum state )
		{
			return state == RequestStateEnum.Canceled;
		}

		protected static bool IsSuccess( RequestStateEnum state )
		{
			return state == RequestStateEnum.Success;
		}
		
		protected static bool NotFound( RequestStateEnum state )
		{
			return state == RequestStateEnum.NotFound;
		}

		public override string ToString()
		{
			return State.ToString();
		}
	}
}