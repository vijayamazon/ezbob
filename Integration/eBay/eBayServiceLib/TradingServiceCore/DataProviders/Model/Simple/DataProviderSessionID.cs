using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Simple
{
	public class DataProviderSessionID : DataProviderSimpleBaseTyped<ResultInfoSessionInfo, ServiceProviderDataInfoRuName>
	{
		public DataProviderSessionID( IEbayServiceProvider serviceProvider )
			: base( serviceProvider)
		{
		}

		public ResultInfoSessionInfo GetSessionId( ServiceProviderDataInfoRuName ruName )
		{
			var req = new GetSessionIDRequestType();
			req.RuName = ruName.Value;
			
			var rez = base.GetServiceData( Service.GetSessionID, req );

			return new ResultInfoSessionInfo( rez );
		}

		public override ResultInfoSessionInfo GetData( ServiceProviderDataInfoRuName ruName )
		{
			return GetSessionId( ruName );
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeSimple.GetSessionId; }
		}
	}
}