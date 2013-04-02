using System.Collections.Generic;
using EzBob.RequestsQueueCore.RequestTasks.States;

namespace EzBob.RequestsQueueCore.RequestStates
{
	internal static class RequestStateFactory
	{
		public static IRequestState Create()
		{
			return RequestStateSingle.Create();
		}

		public static IRequestState Create( IRequestTaskState state )
		{
			return RequestStateSingle.Create( state );
		}

		public static IRequestState Create( IEnumerable<IRequestState> statesList )
		{
			return RequestStateComposite.Create( statesList );
		}
	}
}