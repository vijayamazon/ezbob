using System;
using log4net;

namespace EzBob.CommonLib
{
	public enum WriteLogType
	{
		Error,
		Warning,
		Info,
		Debug
	}

	public static class WriteLoggerHelper
	{
		private static ILog _Log;
		
		private static ILog Log 
		{
			get 
			{
				return _Log ?? ( _Log = LogManager.GetLogger( typeof( WriteLoggerHelper ) ) );
			}
		}

		public static void Write(string message, WriteLogType messageType, ILog log = null, Exception ex = null)
		{
			switch (messageType)
			{
				case WriteLogType.Debug:
					WriteDebug(message, log, ex);
					break;

				case WriteLogType.Error:
					WriteError(message, log, ex);
					break;

				case WriteLogType.Info:
					WriteInfo(message, log, ex);
					break;

				case WriteLogType.Warning:
					WriteWarning(message, log, ex);
					break;

				default:
					throw new NotImplementedException();
			}
		}

		private static void WriteError(string message, ILog log = null, Exception ex = null)
		{
			var logger = log ?? Log;
			logger.Error( message, ex );
		}

		private static void WriteWarning(string message, ILog log = null, Exception ex = null)
		{
			var logger = log ?? Log;
			logger.Warn( message, ex );
		}

		private static void WriteInfo(string message, ILog log = null, Exception ex = null)
		{
			var logger = log ?? Log;
			logger.Info( message, ex );
		}

		private static void WriteDebug( string message, ILog log = null, Exception ex = null )
		{
			var logger = log ?? Log;
			logger.Debug( message, ex );
		}

	}
}