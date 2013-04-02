using EzBob.CommonLib;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos.Orders;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Data;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos.Orders;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant.GetOrders
{
	public class DataProviderGetOrders : DataProviderTokenDependentBase
	{
		public DataProviderGetOrders(DataProviderCreationInfo info) 
			: base(info)
		{
		}

		public static ResultInfoOrders GetOrders( DataProviderCreationInfo info, ParamsDataInfoGetOrdersBase param )
		{
			return new DataProviderGetOrders( info ).GetOrders( param );
		}

		public override CallProcedureType CallProcedureType
		{
			get { return CallProcedureTypeTokenDependent.GetOrders; }
		}

		public ResultInfoOrders GetOrders( ParamsDataInfoGetOrdersBase param )
		{
			var ex = GetOrdersExecutorFactory.Create( param, Info );
			var requestsCounter = new RequestsCounterData();
			var rez = ex.GetOrders( requestsCounter );
			rez.RequestsCounter = requestsCounter;
			return rez;
		}
	}
}