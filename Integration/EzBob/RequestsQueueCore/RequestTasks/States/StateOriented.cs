using System;
using System.Threading;

namespace EzBob.RequestsQueueCore.RequestTasks.States
{
	public class StateOriented<TStateType> : IStateOriented<TStateType>
		where TStateType : class
	{
		#region Events
		public event EventHandler<StateEventArgs<TStateType>> StateChanged;
		#endregion

		#region Fields
		private TStateType _State;
		private TStateType _PrevState;
		#endregion

		#region Properties

		public TStateType State
		{
			get { return _State; }
		}

		public TStateType PrevState
		{
			get { return _PrevState; }
		}

		#endregion

		public virtual void ChangeState( TStateType newState )
		{
			Interlocked.Exchange( ref _State, newState );
			OnStateChanged();			
		}



		#region protected methods
		protected void OnStateChanged()
		{
			if ( StateChanged != null )
			{
				StateChanged( this, new StateEventArgs<TStateType>( _State ) );
			}
		}
		#endregion

	}
}