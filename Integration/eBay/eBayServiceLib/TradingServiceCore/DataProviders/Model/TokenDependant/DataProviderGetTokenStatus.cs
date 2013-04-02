using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant
{
	public class DataProviderGetTokenStatus : DataProviderTokenDependentBase
	{
		public DataProviderGetTokenStatus(DataProviderCreationInfo info) 
			: base(info)
		{
		}

		public ResultInfoTokenStatus GetStatus()
		{
			var req = new GetTokenStatusRequestType();
			var rez = base.GetServiceData( Service.GetTokenStatus, req );
 
			return new ResultInfoTokenStatus(rez);
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeTokenDependent.GetTokenStatus; }
		}
	}
}