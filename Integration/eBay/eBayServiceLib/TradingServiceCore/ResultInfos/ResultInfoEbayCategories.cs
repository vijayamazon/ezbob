using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public class ResultInfoEbayCategories : ResultInfoByServerResponseBase
	{
		private readonly GetCategoriesResponseType _Response;

		public ResultInfoEbayCategories( GetCategoriesResponseType response )
			: base(response)
		{
			_Response = response;

		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.Categories; }
		}
	}
}