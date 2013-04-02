using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public class ResultInfoTikenInfo : ResultInfoByServerResponseBase
	{
		private readonly FetchTokenResponseType _Response;

		public ResultInfoTikenInfo( FetchTokenResponseType response ) 
			: base(response)
		{
			_Response = response;
		}

		public ServiceProviderDataInfoToken Token
		{
			get { return new ServiceProviderDataInfoToken( _Response.eBayAuthToken ); }
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.TokenInfo; }
		}
	}
}