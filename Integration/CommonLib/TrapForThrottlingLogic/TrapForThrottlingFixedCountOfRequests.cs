using System;
using System.Threading;

namespace EzBob.CommonLib.TrapForThrottlingLogic
{
	internal class TrapForThrottlingFixedCountOfRequests : TrapForThrottlingBase
	{

		private readonly string _Name;
		private readonly int _RequestQuota;
		private readonly int _RestoreRateInSeconds;
		private readonly int _LimitAccessRequestQuota;
		private readonly EventWaitHandle _Finish =	new ManualResetEvent( false );

		private readonly object _Locker = new object();
		private int _CurrentRequestInProgress;
		private DateTime? _LastRequestTime;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="settings">settings</param>
		internal TrapForThrottlingFixedCountOfRequests( string name, TrapForThrottlingSettings settings )
			:base(name)
		{
			_Name = name;
			_RequestQuota = settings.RequestQuota;
			_RestoreRateInSeconds = settings.RestoreRateInSeconds;
			_LimitAccessRequestQuota = (int)Math.Round((_RequestQuota / 100d ) * settings.LimitAccessPercentOfRequestQuota, 0, MidpointRounding.AwayFromZero);
		}

		public override T Execute<T>( Func<T> func, string actionName )
		{
			return InternalExecute( func, actionName );
		}

		private T InternalExecute<T>( Func<T> func, string actionName, ActionAccessType accessType = ActionAccessType.Full )
		{
			var limitAccessRequestQuota = accessType == ActionAccessType.Full ? 0 : _LimitAccessRequestQuota;
			var requestQuota = _RequestQuota - limitAccessRequestQuota;

			var rez = default( T );
			var done = false;
			while ( !done )
			{
				var now = DateTime.Now;
				bool hasFree;
				int timeFoFree = 0;
				lock ( _Locker )
				{

					if ( _CurrentRequestInProgress > 0 )
					{
						var elapsedSeconds = (int)( now - _LastRequestTime.Value ).TotalSeconds;

						int reminder;
						var canCellsForFreeCount = Math.DivRem( elapsedSeconds, (int)_RestoreRateInSeconds, out reminder );

						if ( canCellsForFreeCount > 0 )
						{

							_LastRequestTime = now;
							WriteToLog( string.Format( "{0} [{1}: {2}] last restore cells time", _Name, actionName, now ) );
							_CurrentRequestInProgress -= canCellsForFreeCount;

							if ( _CurrentRequestInProgress < 0 )
							{
								_CurrentRequestInProgress = 0;
							}

							WriteToLog( string.Format( "{0} [{1}: {2}] Total buzzy cells: {3} of {4}", _Name, actionName, now, _CurrentRequestInProgress, requestQuota ) );

							if ( _CurrentRequestInProgress == 0 )
							{
								WriteToLog( string.Format( "{0} [{1}: {2}] All cells free", _Name, actionName, now ) );
								_LastRequestTime = null;
							}
						}						
					}

					hasFree = requestQuota - _CurrentRequestInProgress > 0;

					if ( hasFree )
					{
						if ( _CurrentRequestInProgress == 0 )
						{
							WriteToLog( string.Format( "{0} [{1}: {2}] Getting First cells", _Name, actionName, now ) );
							_LastRequestTime = now;
						}

						++_CurrentRequestInProgress;
						WriteToLog( string.Format( "{0} [{1}: {2}] Total buzy cells: {3} of {4}", _Name, actionName, now, _CurrentRequestInProgress, requestQuota ) );
					}
					else
					{
						timeFoFree = _RestoreRateInSeconds - (int)( now - _LastRequestTime.Value ).TotalSeconds;

						WriteToLog( string.Format( "{0} [{1}: {2}] Elapsed time for free Cells: {3} (sec)", _Name, actionName, now, timeFoFree ) );
					}
				}

				if ( hasFree )
				{
					WriteToLog( string.Format( "{0} [{1}: {2}] execute method", _Name, actionName, now ) );
					// execute action
					rez = func();

					done = true;
				}
				else
				{
					WriteToLog( string.Format( "{0} [{1}: {2}] Start wait: {3} (sec)", _Name, actionName, now, timeFoFree ) );
					if ( _Finish.WaitOne( timeFoFree * 1000 ) )
					{
						WriteToLog( string.Format( "{0} [{1}: {2}] Exit raised", _Name, actionName, now ) );
						done = true;
					}
					else
					{
						WriteToLog( string.Format( "{0} [{1}: {2}] End wait", _Name, actionName, now ) );
					}
				}

				done &= !_Finish.WaitOne( 1 );
			}

			return rez;
		}

		public override void Execute(ActionInfo actionInfo)
		{
			var actionName = actionInfo.Name;
			var action = actionInfo.Action;
			ActionAccessType accessType = actionInfo.Access;

			Func<bool> func = () => 
			{ 
				action(); 
				return true; 
			};

			InternalExecute( func, actionName, accessType );
		}

		public override void Exit()
		{
			WriteToLog( string.Format( "TrapForThrottlingFixedCountOfRequests::Exit - Start ({0})", _Name ), WriteLogType.Info );
			_Finish.Set();
			WriteToLog( string.Format( "TrapForThrottlingFixedCountOfRequests::Exit - Finish ({0})", _Name ), WriteLogType.Info );
		}

	}
}
