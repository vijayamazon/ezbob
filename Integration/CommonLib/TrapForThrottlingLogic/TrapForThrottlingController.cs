using System;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;

namespace EzBob.CommonLib.TrapForThrottlingLogic
{

	public static class TrapForThrottlingController
	{
		private static readonly ILog _Log = LogManager.GetLogger( typeof( TrapForThrottlingController ) );
		private static readonly object _Locker = new object();
		private static readonly List<ITrapForThrottling> _Items = new List<ITrapForThrottling>();

		/// <summary>
		/// Create TrapForThrottling
		/// </summary>
		/// <param name="name"></param>
		/// <param name="requestQuota">Request Quota</param>
		/// <param name="restoreRateInSeconds">Restore one element in time (seconds)</param>
		/// <param name="limitAccessPercentOfRequestQuota"></param>
		/// <returns></returns>
		public static ITrapForThrottling Create( string name, int requestQuota, int restoreRateInSeconds = 60, int limitAccessPercentOfRequestQuota = 30 )
		{
			WriteToLog( string.Format( "TrapForThrottling::Create ({0})", name ), WriteLogType.Info );
			var item = new TrapForThrottlingFixedCountOfRequests( name, new TrapForThrottlingSettings( requestQuota, restoreRateInSeconds, limitAccessPercentOfRequestQuota ) );

			return AddItemToStorage( item );			
		}

		private static ITrapForThrottling AddItemToStorage( ITrapForThrottling item )
		{
			lock ( _Locker )
			{
				_Items.Add( item );
			}

			return item;
		}

		public static ITrapForThrottling CreateSimpleWait( string name, int requestQuota, RequestQuoteTimePeriodType requestQuoteTimePeriodType )
		{
			WriteToLog( string.Format( "TrapForThrottling::CreateSimpleWait ({0})", name ), WriteLogType.Info );
			var item = new TrapForThrottlingSimpleWait( name, requestQuota, requestQuoteTimePeriodType );

			return AddItemToStorage( item );			
		}

		public static void Exit()
		{
			WriteToLog( "TrapForThrottlingController::Exit - Start", WriteLogType.Info );
			lock ( _Locker )
			{
				foreach (var trapForThrottling in _Items)
				{
					trapForThrottling.Exit();
				}
			}
			WriteToLog( "TrapForThrottlingController::Exit  - Finish", WriteLogType.Info );
		}

		private static void WriteToLog( string message, WriteLogType messageType = WriteLogType.Debug, Exception ex = null )
		{
			WriteLoggerHelper.Write(message, messageType, null, ex);
			Debug.WriteLine( message );
		}
	}
}