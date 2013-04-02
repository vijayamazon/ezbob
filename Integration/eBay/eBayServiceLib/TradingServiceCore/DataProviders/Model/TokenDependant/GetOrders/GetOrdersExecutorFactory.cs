using System;
using EzBob.eBayServiceLib.Common;
using EzBob.eBayServiceLib.TradingServiceCore.DataInfos.Orders;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant.GetOrders
{
	internal static class GetOrdersExecutorFactory
	{
		public static IGetOrdersExecutor Create(ParamsDataInfoGetOrdersBase param, DataProviderCreationInfo info)
		{
			switch ( param.Type )
			{
				case ParamsDataInfoGetOrdersParamsType.Simple:
					return new GetOrdersExecutorSimple( param as ParamsDataInfoGetOrdersSimple, info );

				case ParamsDataInfoGetOrdersParamsType.FromDateToDateCreated:
					return new GetOrdersExecutorFromDateToDateCreated( param as ParamsDataInfoGetOrdersFromDateToDateBase, info );

				case ParamsDataInfoGetOrdersParamsType.FromDateToDateModified:
					return new GetOrdersExecutorFromDateToDateModified( param as ParamsDataInfoGetOrdersFromDateToDateBase, info );

				default:
					throw new NotImplementedException();
			}
			
		}
	}
}