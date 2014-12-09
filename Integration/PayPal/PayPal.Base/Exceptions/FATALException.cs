using System;
using log4net;

namespace PayPal.Platform.SDK
{
	/// <summary>
	/// Custom FATALException holds Short and Long messages.
	/// </summary>
	public class FATALException : Exception
	{

        private static readonly ILog log = LogManager.GetLogger("PAYPALLOGFILE");
		/// <summary>
		/// Short message
		/// </summary>
		private string FATALExMessage ;
		/// <summary>
		/// Long message
		/// </summary>
		private string FATALExpLongMessage ;

		public FATALException(string FATALExceptionMessage, Exception exception)
		{
			/// Logging the exception.
			if (log.IsInfoEnabled)
			{	
				log.Info(FATALExceptionMessage, exception);
			}

			this.FATALExMessage = FATALExceptionMessage;
			this.FATALExpLongMessage = exception.Message ;
		}

		/// <summary>
		/// Short message.
		/// </summary>
		public string FATALExceptionMessage
		{
			get
			{
				return FATALExMessage; 
			}
			set
			{
				FATALExMessage = value;
			}
		}

		/// <summary>
		/// Long message
		/// </summary>
		public string FATALExceptionLongMessage
		{
			get
			{
				return FATALExpLongMessage; 
			}
			set
			{
				FATALExpLongMessage = value;
			}
		}

	    public override string ToString()
	    {
            return string.Format("FATALExceptionMessage: {1}, FATALExceptionLongMessage: {2}{3}{0}", base.ToString(), FATALExceptionMessage, FATALExceptionLongMessage, Environment.NewLine);
	    }

	}

}
