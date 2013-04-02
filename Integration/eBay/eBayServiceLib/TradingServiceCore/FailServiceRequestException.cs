using System;
using System.Linq;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore
{
	public class FailServiceRequestException : Exception
	{
		public FailServiceRequestException(AbstractResponseType response)
		{
			ResultInfoError = new ResultInfoError( response );
		}

		public ResultInfoError ResultInfoError { get; private set; }

		public override string Message
		{
			get
			{
				string rez = string.Empty;
				if ( ResultInfoError.Errors.Length == 1 )
				{
					rez = ResultInfoError.Errors[0].LongMessage;
				}
				else
				{
					var errs = ResultInfoError.Errors.Select((t, i) => string.Format("{0}.{1}", i + 1, t.LongMessage)).ToList();

					rez = string.Join("\n", errs);
				}

				return string.Format( "Error on Service: {0}\n\t{1}", new eBayServiceInfo().DisplayName, rez );
			}
		}
	}
}