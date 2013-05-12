using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;

namespace Integration.Play {
	internal class PlayOrdersAggregatorFactory
		: DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<PlayOrderItem>, PlayOrderItem, PlayDatabaseFunctionType>
	{
		public override
			DataAggregatorBase<ReceivedDataListTimeDependentInfo<PlayOrderItem>, PlayOrderItem, PlayDatabaseFunctionType>
		CreateDataAggregator(
			ReceivedDataListTimeDependentInfo<PlayOrderItem> data,
			ICurrencyConvertor currencyConverter
		) {
			return new PlayOrdersAgregator(data, currencyConverter);
		} // CreateDataAggregator
	} // class PlayOrdersAggregatorFactory

	internal class PlayOrdersAgregator
		: DataAggregatorBase<ReceivedDataListTimeDependentInfo<PlayOrderItem>, PlayOrderItem, PlayDatabaseFunctionType>
	{
		public PlayOrdersAgregator(
			ReceivedDataListTimeDependentInfo<PlayOrderItem> orders, ICurrencyConvertor currencyConvertor
		): base(orders, currencyConvertor)
		{} // constructor

		private int GetShipedOrdersCount(IEnumerable<PlayOrderItem> orders) {
			// TODO: update this once Channel Grabber implement order status.
			// Currently returns all the orders.
			return orders.Count();
		} // GetShippedOrdersCount

		private double GetAverageSumOfOrder(IEnumerable<PlayOrderItem> orders) {
			double sum   = GetTotalSumOfOrders(orders);
			int    count = GetShipedOrdersCount(orders);

			return count == 0 ? 0 : sum / count;
		} // GetAverageSumOfOrders

		private double GetTotalSumOfOrders(IEnumerable<PlayOrderItem> orders) {
			// TODO: update this once Channel Grabber implement order status.
			// Currently returns all the orders.

			return orders
				.Where(o => o.TotalCost.HasValue)
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
			PlayDatabaseFunctionType functionType,
			IEnumerable<PlayOrderItem> orders
		) {
			switch (functionType) {
			case PlayDatabaseFunctionType.AverageSumOfOrder:
				return GetAverageSumOfOrder(orders);

			case PlayDatabaseFunctionType.NumOfOrders:
				return GetShipedOrdersCount(orders);

			case PlayDatabaseFunctionType.TotalSumOfOrders:
				return GetTotalSumOfOrders(orders);
			
			default:
				throw new NotImplementedException();
			} // switch
		} // InternalCalculateAggregatorValue
	} // class PlayOrdersAgregator
} // namespace