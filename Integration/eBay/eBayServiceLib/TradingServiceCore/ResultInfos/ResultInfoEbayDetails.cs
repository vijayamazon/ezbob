using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public class ResultInfoEbayDetails : ResultInfoByServerResponseBase
	{
		private readonly GeteBayDetailsResponseType _Response;

		public ResultInfoEbayDetails(GeteBayDetailsResponseType response) 
			: base(response)
		{
			_Response = response;
			
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.EbayDetails; }
		}
	}
}