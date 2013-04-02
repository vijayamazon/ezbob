using System;
using System.Diagnostics;

namespace EzBob.CommonLib
{
	public static class ElapsedTimeHelper
	{
		private static readonly Stopwatch _Stopwatch;

		static ElapsedTimeHelper()
		{
			_Stopwatch = new Stopwatch();
		}

		public static void CalculateAndStoreElapsedTimeForCallInSeconds( ElapsedTimeInfo elapsedTimeInfo, ElapsedDataMemberType elapsedDataMemberType, Action action )
		{
			CalculateAndStoreElapsedTimeForCallInSeconds( elapsedTimeInfo, elapsedDataMemberType, () =>
				                                                                                      {
					                                                                                      action();
					                                                                                      return true;
				                                                                                      } );
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