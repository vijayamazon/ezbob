using System.Diagnostics;
using System.Linq;

namespace EzBob.CommonLib
{
	public class WaitBeforeRetryHelper
	{
		private readonly IErrorRetryingWaiter _Waiter;
		
		public WaitBeforeRetryHelper(IErrorRetryingWaiter waiter)
		{
			_Waiter = waiter;
			waiter.AssignHelper( this );
		}

		public int CountIteration { get; private set; }
		public int CountRetrying { get; private set; }		

		public bool IncreaseAndWait( ErrorRetryingInfo errorRetryingInfo )
		{
			if ( errorRetryingInfo == null ||
			     errorRetryingInfo.Info == null ||
			     errorRetryingInfo.Info.Length == 0 ||
			     CountIteration >= errorRetryingInfo.Info.Length )
			{
				WriteToLog( "No need wait" );
				_Waiter.Reset();
				return false;
			}
			var infos = errorRetryingInfo.Info.OrderBy( i => i.Index ).ToArray();

			var info = infos[CountIteration];

			if ( info.CountRequestsExpectError == CountRetrying )
			{
				CountRetrying = 0;
				++CountIteration;

				if ( CountIteration == infos.Length )
				{
					WriteToLog( "Exit wait cycle");
					_Waiter.Reset();
					return false;
				}
			}
			++CountRetrying;

			if ( info.CountRequestsExpectError == CountRetrying && CountIteration + 1 == infos.Length && !errorRetryingInfo.UseLastTimeOut )
			{
				_Waiter.Reset();
				return false;
			}

			var timeOutInMinutes = info.CountRequestsExpectError == CountRetrying ? info.TimeOutAfterRetryingExpiredInMinutes : errorRetryingInfo.MinorTimeoutInSeconds / 60d;
			WriteToLog( string.Format( "Iteration #: {0},\tRetrying #:{1},\tStart Wait: {2} (Minute-s)", CountIteration, CountRetrying, timeOutInMinutes ) );			
			_Waiter.Wait(timeOutInMinutes);
			WriteToLog( string.Format( "Iteration #: {0},\tRetrying #:{1},\tEnd Waiting: {2} (Minute-s)", CountIteration, CountRetrying, timeOutInMinutes ) );			
			return true;

		}

		private void WriteToLog( string message )
		{
			WriteLoggerHelper.Write( message, WriteLogType.Debug );
			Debug.WriteLine( message );
		}
	}
}