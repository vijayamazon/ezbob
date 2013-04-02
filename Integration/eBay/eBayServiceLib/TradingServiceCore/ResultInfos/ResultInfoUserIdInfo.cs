using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.ResultInfos
{
	public class ResultInfoUserIdInfo : ResultInfoByServerResponseBase
	{
		private readonly ConfirmIdentityResponseType _Response;

		public ResultInfoUserIdInfo(ConfirmIdentityResponseType response) 
			: base(response)
		{
			_Response = response;			
		}

		public ServiceProviderDataInfoUserId Id
		{
			get { return new ServiceProviderDataInfoUserId( _Response.UserID ); }
		}

		public override DataInfoTypeEnum DataInfoType
		{
			get { return DataInfoTypeEnum.UserIdInfo; }
		}
	}
}