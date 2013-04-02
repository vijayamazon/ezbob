using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public class ResultInfoError : ResultInfoByServerResponseBase
	{
		public ResultInfoError( AbstractResponseType response )
			: base( response )
		{
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.ErrorInfo; }
		}
	}
}