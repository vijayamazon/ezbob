using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant
{
	public class DataProviderGetAccount : DataProviderTokenDependentBase
	{
		public DataProviderGetAccount(DataProviderCreationInfo info)
			: base(info)
		{
		}

		public ResultInfoEbayAccount GetAccount()
		{
			var req = new GetAccountRequestType();

			GetAccountResponseType response = base.GetServiceData(Service.GetAccount, req);
			var rez = new ResultInfoEbayAccount(response);
			rez.IncrementRequests("GetAccount");
			return rez;
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeTokenDependent.GetAccount; }
		}
	}
}