using System;
using System.Diagnostics;
using EzBob.CommonLib;
using EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.Base;
using EzBob.eBayServiceLib.TradingServiceCore.ResultInfos.Orders;
using EzBob.eBayServiceLib.com.ebay.developer.soap;
using log4net;

namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant.GetOrders
{
	abstract class GetOrdersExecutorBase : IGetOrdersExecutor
	{
		private static readonly ILog _Log = LogManager.GetLogger( typeof( GetOrdersExecutorBase ) );

		protected GetOrdersExecutorBase( DataProviderCreationInfo info)
		{
			DataProvider = new InternalDataProviderGetOrders( info );
		}

		private InternalDataProviderGetOrders DataProvider { get; set; }

		public abstract ResultInfoOrders GetOrders(RequestsCounterData requestsCounter);

		protected GetOrdersResponseType GetOrders( GetOrdersRequestType req, RequestsCounterData requestsCounter )
		{
			return DataProvider.GetOrders( req, requestsCounter );
		}

		protected void WriteToLog( string message, params object[] args )
		{
			WriteToLog( string.Format( message, args ) );
		}

		protected void WriteToLog( string message, WriteLogType messageType = WriteLogType.Debug, Exception ex = null )
		{
			WriteLoggerHelper.Write(message, messageType, null, ex);
			Debug.WriteLine( message );
		}
	}
}