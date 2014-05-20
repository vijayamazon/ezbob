namespace EzBob.eBayServiceLib.TradingServiceCore.DataProviders.Model.TokenDependant.GetOrders
{
	using System;
	using System.Diagnostics;
	using CommonLib;
	using Base;
	using ResultInfos.Orders;
	using com.ebay.developer.soap;

	abstract class GetOrdersExecutorBase : IGetOrdersExecutor
	{
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