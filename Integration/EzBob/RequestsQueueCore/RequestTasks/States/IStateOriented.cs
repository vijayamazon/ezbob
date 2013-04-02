using System;

namespace EzBob.RequestsQueueCore.RequestTasks.States
{
	public interface IStateOriented<TStateType>
	{
		event EventHandler<StateEventArgs<TStateType>> StateChanged;
	}
}