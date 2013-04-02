using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Simple
{
	public class DataProviderFetchToken : DataProviderSimpleBaseTyped<ResultInfoTikenInfo, ServiceProviderDataInfoSessionId>
	{
		public DataProviderFetchToken( IEbayServiceProvider serviceProvider )
			: base( serviceProvider)
		{
		}

		public ResultInfoTikenInfo GenerateToken( ServiceProviderDataInfoSessionId sesionId )
		{
			var req = new FetchTokenRequestType();
			req.SessionID = sesionId.Value;
			req.DetailLevel = new[] { DetailLevelCodeType.ReturnAll };
			
			var rez = base.GetServiceData( Service.FetchToken, req );

			return new ResultInfoTikenInfo(rez);
		}

		public override ResultInfoTikenInfo GetData( ServiceProviderDataInfoSessionId sesionId )
		{
			return GenerateToken( sesionId );
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeSimple.FetchToken; }
		}
	}
}