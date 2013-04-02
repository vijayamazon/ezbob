using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public class ResultInfoSessionInfo : ResultInfoByServerResponseBase
	{
		private readonly GetSessionIDResponseType _Response;

		public ResultInfoSessionInfo( GetSessionIDResponseType response )
			: base( response )
		{
			_Response = response;
		}

		public ServiceProviderDataInfoSessionId SessionId
		{
			get { return new ServiceProviderDataInfoSessionId( _Response.SessionID ); }
		}
		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.SessionInfo; }
		}
	}
}