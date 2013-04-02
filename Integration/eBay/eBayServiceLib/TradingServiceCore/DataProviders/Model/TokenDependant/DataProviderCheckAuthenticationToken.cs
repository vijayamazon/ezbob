using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant
{
	public class DataProviderCheckAuthenticationToken : DataProviderTokenDependentBase
	{
		public DataProviderCheckAuthenticationToken( DataProviderCreationInfo info )
			: base( info )
		{
		}

		public ResultInfoCheckAuthenticationToken Check()
		{
			var req = new GeteBayOfficialTimeRequestType();
			var rez = base.GetServiceData( Service.GeteBayOfficialTime, req );

			return new ResultInfoCheckAuthenticationToken( rez );
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeTokenDependent.GeteBayOfficialTime; }
		}
	}
}