using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Simple
{
	public class DataProviderConfirmIdentity : DataProviderSimpleBaseTyped<ResultInfoUserIdInfo, ServiceProviderDataInfoSessionId>
	{
		public DataProviderConfirmIdentity( IEbayServiceProvider serviceProvider)
			: base( serviceProvider)
		{
		}

		public ResultInfoUserIdInfo GetUserId( ServiceProviderDataInfoSessionId sesionId )
		{
			var req = new ConfirmIdentityRequestType();
			req.SessionID = sesionId.Value;			
			
			ConfirmIdentityResponseType rez = base.GetServiceData( Service.ConfirmIdentity, req );
			return new ResultInfoUserIdInfo( rez );
		}

		public override ResultInfoUserIdInfo GetData(ServiceProviderDataInfoSessionId param)
		{
			return GetUserId( param );
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeSimple.ConfirmIdentity; }
		}
	}
}