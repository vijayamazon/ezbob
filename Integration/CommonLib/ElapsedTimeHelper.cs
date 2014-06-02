using System;
using System.Diagnostics;

namespace EzBob.CommonLib
{
	using Ezbob.Utils;
	using log4net;

	public static class ElapsedTimeHelper
	{
		private static readonly Stopwatch _Stopwatch;
		private static readonly ILog Log = LogManager.GetLogger(typeof(ElapsedTimeHelper));

		static ElapsedTimeHelper()
		{
			_Stopwatch = new Stopwatch();
		}

		public static void CalculateAndStoreElapsedTimeForCallInSeconds( ElapsedTimeInfo elapsedTimeInfo, ElapsedDataMemberType elapsedDataMemberType, Action action )
		{
			Log.DebugFormat("{1} {0} begin", elapsedDataMemberType.ToString(), action.Method.Name);
			CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo, elapsedDataMemberType, () =>
				                                                                                      {
					                                                                                      action();
					                                                                                      return true;
				                                                                                      } );
			Log.DebugFormat("{1} {0} end", elapsedDataMemberType.ToString(), action.Method.Name);
		}

		public static T CalculateAndStoreElapsedTimeForCallInSeconds<T>( ElapsedTimeInfo elapsedTimeInfo, ElapsedDataMemberType elapsedDataMemberType, Func<T> func )
		{
			return CalculateAndStoreElapsedTimeForCallInSeconds( _Stopwatch, elapsedTimeInfo, elapsedDataMemberType, func );
		}

		private static T CalculateAndStoreElapsedTimeForCallInSeconds<T>( Stopwatch stopwatch, ElapsedTimeInfo elapsedTimeInfo, ElapsedDataMemberType elapsedDataMemberType, Func<T> func )
		{
			if ( stopwatch == null )
			{
				stopwatch = Stopwatch.StartNew();
			}
			else
			{
				stopwatch.Restart();
			}

			var rez = func();

			stopwatch.Stop();

			var totalSeconds = (int)Math.Round( stopwatch.Elapsed.TotalSeconds, MidpointRounding.AwayFromZero );
			elapsedTimeInfo.IncreateData( elapsedDataMemberType, totalSeconds );

			return rez;
		}
	}
}