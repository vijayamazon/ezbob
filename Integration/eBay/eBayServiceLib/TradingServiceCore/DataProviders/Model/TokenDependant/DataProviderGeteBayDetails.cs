using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using EzBob.eBayServiceLib.com.ebay.developer.soap;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant
{
	public class DataProviderGeteBayDetails : DataProviderTokenDependentBase
	{
		public DataProviderGeteBayDetails(DataProviderCreationInfo info) 
			: base(info)
		{
		}

		public ResultInfoEbayDetails GetDetails(DetailNameCodeType[] detailNameCodeType = null)
		{
			var req = new GeteBayDetailsRequestType();
			req.DetailName = detailNameCodeType;

			var rez = base.GetServiceData(Service.GeteBayDetails, req);
			
			return new ResultInfoEbayDetails(rez);
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeTokenDependent.GeteBayDetails; }
		}
	}
}