using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public class ErrorInfo
	{
		private readonly ErrorType _ErrorType;

		public ErrorInfo( ErrorType errorType )
		{
			_ErrorType = errorType;
		}

		public string ErrorCode
		{
			get { return _ErrorType.ErrorCode; }
		}

		public SeverityCodeType SeverityCode
		{
			get { return _ErrorType.SeverityCode; }
		}

		public string ShortMessage
		{
			get { return _ErrorType.ShortMessage; }
		}

		public string LongMessage
		{
			get { return _ErrorType.LongMessage; }
		}
	}
}