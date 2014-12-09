using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace EzBob.CommonLib
{
	[Serializable]
	public class ErrorRetryingInfo	
	{
		public ErrorRetryingInfo()
			:this(true)
		{
		}

		public ErrorRetryingInfo(bool enableRetrying, int minorTimeoutInSeconds = 60, bool useLastTimeOut = false)
		{
			UseLastTimeOut = useLastTimeOut;
			MinorTimeoutInSeconds = minorTimeoutInSeconds;
			EnableRetrying = enableRetrying;
		}

		public ErrorRetryingInfo( int countIteration, int countRetryingInAnyIteration, int timeoutInAnyIteration )
			:this()
		{
			var list = new List<ErrorRetryingItemInfo>();

			for (int i = 0; i < countIteration; i++)
			{
				list.Add( new ErrorRetryingItemInfo( i+1, countRetryingInAnyIteration, timeoutInAnyIteration ) );
			}

			Info = list.ToArray();
		}

		[XmlElement("IterationSettings")]
		public ErrorRetryingItemInfo[] Info { get; set; }

		/// <summary>
		/// Use Last Time Out
		/// </summary>
		/// <remarks>
		/// Default value is 'False'
		/// </remarks>
		[XmlAttribute]
		public bool UseLastTimeOut { get; set; }

		/// <summary>
		/// Minor Timeout In Seconds
		/// </summary>
		/// <remarks>
		/// Default value is 60 seconds - 1 minute
		/// </remarks>
		[XmlAttribute]
		public int MinorTimeoutInSeconds { get; set; }

		/// <summary>
		/// Enable Retrying
		/// </summary>
		/// <remarks>
		/// Enable/disable retrying mechanism wile error occurred
		/// Default value is 'True' - On
		/// </remarks>
		[XmlAttribute]
		public bool EnableRetrying { get; set; }
	}

	[Serializable]
	public class ErrorRetryingItemInfo
	{
		public ErrorRetryingItemInfo()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="index">Index</param>
		/// <param name="countRequestsExpectError">Count Requests Expect Error</param>
		/// <param name="timeOutAfterRetryingExpiredInMinutes">Timeout After Retrying Expired In Minutes</param>
		public ErrorRetryingItemInfo(int index,  int countRequestsExpectError, int timeOutAfterRetryingExpiredInMinutes )
		{
			Index = index;
			CountRequestsExpectError = countRequestsExpectError;
			TimeOutAfterRetryingExpiredInMinutes = timeOutAfterRetryingExpiredInMinutes;
		}

		[XmlAttribute]		
		public int Index { get; set; }

		[XmlAttribute]		
		public int CountRequestsExpectError { get; set; }

		[XmlAttribute]
		public int TimeOutAfterRetryingExpiredInMinutes { get; set; }

		public override string ToString()
		{
			return string.Format( "Iter #{0} -  Retry Count: {1} (Timeout after: {2})", Index, CountRequestsExpectError, TimeOutAfterRetryingExpiredInMinutes );
		}
	}
}
