using System;
using System.Threading;

namespace EzBob.CommonLib.TrapForThrottlingLogic
{
	public enum RequestQuoteTimePeriodType
	{
		PerSecond,
		PerMinute
	}

	internal class TrapForThrottlingSimpleWait : TrapForThrottlingBase
	{
		private readonly object _Locker = new object();
		private readonly EventWaitHandle _Finish = new ManualResetEvent( false );
		private readonly TimeSpan _RunWithIntervalInSeconds;
		private DateTime _LastRequest;

		internal TrapForThrottlingSimpleWait( string name, int requestQuota, RequestQuoteTimePeriodType requestQuoteTimePeriodType )
			:base(name)
		{
			_LastRequest = DateTime.Now;

			switch (requestQuoteTimePeriodType)
			{
				case RequestQuoteTimePeriodType.PerMinute:
					_RunWithIntervalInSeconds = new TimeSpan( (long)( ( (60 * 10e6) / requestQuota ) ) );
					break;
				case RequestQuoteTimePeriodType.PerSecond:
					_RunWithIntervalInSeconds = new TimeSpan( (long)( ( 10e6 / requestQuota ) ) );
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public TimeSpan RunWithIntervalInSeconds
		{
			get { return _RunWithIntervalInSeconds; }
		}

		private T InternalExecute<T>( Func<T> func, string actionName )
		{
			var rez = default( T );
			var done = false;

			while ( !done )
			{
				var isFree = false;
				var now = DateTime.Now;
				var timeFoFree = new TimeSpan();
				lock ( _Locker )
				{

					var timeDiff = _LastRequest - now + _RunWithIntervalInSeconds;

					isFree = timeDiff.TotalMilliseconds < 0;

					if ( isFree )
					{
						_LastRequest = DateTime.Now;
					}
					else
					{
						timeFoFree = timeDiff;
					}
				}

				if ( isFree )
				{
					WriteToLog( string.Format( "{0} [{1}: {2}] execute method", Name, actionName, now ) );
					rez = func();
					done = true;
				}
				else
				{
					WriteToLog( string.Format( "{0} [{1}: {2}] Start wait: {3} (millisecends)", Name, actionName, now, timeFoFree.TotalMilliseconds ) );
					if ( _Finish.WaitOne( timeFoFree ) )
					{
						WriteToLog( string.Format( "{0} [{1}: {2}] Exit raised", Name, actionName, now ) );
						done = true;
					}
					else
					{
						WriteToLog( string.Format( "{0} [{1}: {2}] End wait", Name, actionName, now ) );
					}
				}

				done &= !_Finish.WaitOne( 1 );
			}

			return rez;
		}

		public override T Execute<T>( Func<T> func, string actionName )
		{
			return InternalExecute( func, actionName );
		}

		public override void Execute(ActionInfo actionInfo)
		{
			var action = actionInfo.Action;
			var actionName = actionInfo.Name;

			Func<bool> func = () =>
			{
				action();
				return true;
			};

			InternalExecute( func, actionName );
		}

		public override void Exit()
		{
			WriteToLog( string.Format( "TrapForThrottlingSimpleWait::Exit - Start ({0})", Name ), WriteLogType.Info );
			_Finish.Set();
			WriteToLog( string.Format( "TrapForThrottlingSimpleWait::Exit - Finish ({0})", Name ), WriteLogType.Info );
		}
	}

}
