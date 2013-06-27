using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;
using Integration.ChannelGrabberConfig;

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
			return new OrdersAgregator(data, currencyConverter);
		} // CreateDataAggregator
	} // class OrdersAggregatorFactory

	internal class OrdersAgregator
		: DataAggregatorBase<ReceivedDataListTimeDependentInfo<ChannelGrabberOrderItem>, ChannelGrabberOrderItem, FunctionType>
	{
		public OrdersAgregator(
			ReceivedDataListTimeDependentInfo<ChannelGrabberOrderItem> orders, ICurrencyConvertor currencyConvertor
		) : base(orders, currencyConvertor) {
		} // constructor

		private int GetShipedOrdersCount(IEnumerable<ChannelGrabberOrderItem> orders) {
			return orders.Count(o => o.OrderStatus == "Dispatched");
		} // GetShippedOrdersCount

		private double GetAverageSumOfOrder(IEnumerable<ChannelGrabberOrderItem> orders) {
			int    count = GetShipedOrdersCount(orders);
			double sum   = count == 0 ? 0 : GetTotalSumOfOrders(orders);

			return count == 0 ? 0 : sum / count;
		} // GetAverageSumOfOrders

		private double GetTotalSumOfOrders(IEnumerable<ChannelGrabberOrderItem> orders) {
			return orders
				.Where(o => o.TotalCost.HasValue && o.OrderStatus == "Dispatched")
				.Sum(
					o =>
					CurrencyConverter.ConvertToBaseCurrency(
						o.CurrencyCode,
						(double)o.TotalCost,
						o.PurchaseDate
					).Value
				);
		} // GetTotalSumOfOrders

		protected override object InternalCalculateAggregatorValue(
			FunctionType functionType,
			IEnumerable<ChannelGrabberOrderItem> orders
		) {
			switch (functionType) {
			case FunctionType.AverageSumOfOrders:
				return GetAverageSumOfOrder(orders);

			case FunctionType.NumOfOrders:
				return GetShipedOrdersCount(orders);

			case FunctionType.TotalSumOfOrders:
				return GetTotalSumOfOrders(orders);
			
			default:
				throw new NotImplementedException();
			} // switch
		} // InternalCalculateAggregatorValue
	} // class OrdersAgregator
} // namespace