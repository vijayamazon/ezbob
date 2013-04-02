using System.Linq;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public abstract class ResultInfoByServerResponseBase : EbayResultInfoBase
	{
		protected ResultInfoByServerResponseBase( AbstractResponseType response )
			:base(response.Timestamp)
		{
			if ( response != null && response.Errors != null )
			{
				AddErrors(response.Errors.Select(e => new ErrorInfo(e)));
			}
		}
	}
}