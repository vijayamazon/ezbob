using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;
using Integration.ChannelGrabberConfig;
using log4net;

namespace Integration.ChannelGrabberFrontend {
	public class OrdersAggregatorFactory
		: DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<ChannelGrabberOrderItem>, ChannelGrabberOrderItem, FunctionType>
	{
		public override
			DataAggregatorBase<ReceivedDataListTimeDependentInfo<ChannelGrabberOrderItem>, ChannelGrabberOrderItem, FunctionType>
		CreateDataAggregator(
			ReceivedDataListTimeDependentInfo<ChannelGrabberOrderItem> data,
			ICurrencyConvertor currencyConverter
		) {
			ms_oLog.Debug("OrdersAggregatorFactory.CreateDataAggregator start");

			var oa = new OrdersAgregator(data, currencyConverter);

			ms_oLog.Debug("OrdersAggregatorFactory.CreateDataAggregator end");

			return oa;
		} // CreateDataAggregator

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(OrdersAggregatorFactory));
	} // class OrdersAggregatorFactory

	internal class OrdersAgregator
		: DataAggregatorBase<ReceivedDataListTimeDependentInfo<ChannelGrabberOrderItem>, ChannelGrabberOrderItem, FunctionType>
	{
		public OrdersAgregator(
			ReceivedDataListTimeDependentInfo<ChannelGrabberOrderItem> orders, ICurrencyConvertor currencyConvertor
		) : base(orders, currencyConvertor) {
			ms_oLog.Debug("OrdersAgregator constructed");
		} // constructor

		private int GetShipedOrdersCount(IEnumerable<ChannelGrabberOrderItem> orders) {
			ms_oLog.Debug("start");

			var nCount = orders.Count(o => o.OrderStatus == "Dispatched");

			ms_oLog.Debug("end");

			return nCount;
		} // GetShippedOrdersCount

		private double GetAverageSumOfOrder(IEnumerable<ChannelGrabberOrderItem> orders) {
			ms_oLog.Debug("start");

			int    count = GetShipedOrdersCount(orders);
			double sum   = count == 0 ? 0 : GetTotalSumOfOrders(orders);

			var nAvg = count == 0 ? 0 : sum / count;

			ms_oLog.Debug("end");

			return nAvg;
		} // GetAverageSumOfOrders

		private double GetTotalSumOfOrders(IEnumerable<ChannelGrabberOrderItem> orders) {
			ms_oLog.Debug("start");

			var s = orders
				.Where(o => o.TotalCost.HasValue && o.OrderStatus == "Dispatched")
				.Sum(
					o =>
					CurrencyConverter.ConvertToBaseCurrency(
						o.CurrencyCode,
						(double)o.TotalCost,
						o.PurchaseDate
					).Value
				);

			ms_oLog.Debug("end");

			return s;
		} // GetTotalSumOfOrders

		protected override object InternalCalculateAggregatorValue(
			FunctionType functionType,
			IEnumerable<ChannelGrabberOrderItem> orders
		) {
			ms_oLog.Debug("start");

			switch (functionType) {
			case FunctionType.AverageSumOfOrders:
				ms_oLog.Debug("end");
				return GetAverageSumOfOrder(orders);

			case FunctionType.NumOfOrders:
				ms_oLog.Debug("end");
				return GetShipedOrdersCount(orders);

			case FunctionType.TotalSumOfOrders:
				ms_oLog.Debug("end");
				return GetTotalSumOfOrders(orders);
			
			default:
				ms_oLog.Debug("exception");
				throw new NotImplementedException();
			} // switch
		} // InternalCalculateAggregatorValue

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(OrdersAgregator));
	} // class OrdersAgregator
} // namespace