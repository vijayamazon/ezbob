using EzBob.CommonLib;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos.Orders;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant.GetOrders
{
	internal interface IGetOrdersExecutor
	{
		ResultInfoOrders GetOrders(RequestsCounterData requestsCounter);
	}
}