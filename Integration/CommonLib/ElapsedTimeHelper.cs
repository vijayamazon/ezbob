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

		public static void CalculateAndStoreElapsedTimeForCallInSeconds(ElapsedTimeInfo elapsedTimeInfo, int mpId, ElapsedDataMemberType elapsedDataMemberType, Action action)
		{

			CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo, mpId, elapsedDataMemberType,
				() => { action(); return true; });

		}

		public static T CalculateAndStoreElapsedTimeForCallInSeconds<T>(ElapsedTimeInfo elapsedTimeInfo, int mpId, ElapsedDataMemberType elapsedDataMemberType, Func<T> func)
		{
			return CalculateAndStoreElapsedTimeForCallInSeconds(_Stopwatch, elapsedTimeInfo, mpId, elapsedDataMemberType, func);
		}

		private static T CalculateAndStoreElapsedTimeForCallInSeconds<T>(Stopwatch stopwatch, ElapsedTimeInfo elapsedTimeInfo, int mpId, ElapsedDataMemberType elapsedDataMemberType, Func<T> func)
		{
			Log.DebugFormat("CalculateAndStoreElapsedTimeForCallInSeconds {0} for mp {1} begin", elapsedDataMemberType.ToString(), mpId);
			if (stopwatch == null)
			{
				stopwatch = Stopwatch.StartNew();
			}
			else
			{
				stopwatch.Restart();
			}

			var rez = func();

			stopwatch.Stop();

			var totalSeconds = (int)Math.Round(stopwatch.Elapsed.TotalSeconds, MidpointRounding.AwayFromZero);
			elapsedTimeInfo.IncreateData(elapsedDataMemberType, totalSeconds);

			Log.DebugFormat("CalculateAndStoreElapsedTimeForCallInSeconds {0} for mp {1} end, took {2} sec", elapsedDataMemberType.ToString(), mpId, totalSeconds);
			return rez;
		}
	}
}