using System;

namespace EzBob.RequestsQueueCore.RequestTasks.States
{
	public class StateEventArgs<T> : EventArgs
	{
		public T State { get; private set; }

		public StateEventArgs( T state )
		{
			State = state;
		}
	}
}