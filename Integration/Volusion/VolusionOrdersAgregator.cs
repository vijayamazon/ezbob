using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;

namespace Integration.Volusion {
	internal class VolusionOrdersAggregatorFactory
		: DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<VolusionOrderItem>, VolusionOrderItem, VolusionDatabaseFunctionType>
	{
		public override
			DataAggregatorBase<ReceivedDataListTimeDependentInfo<VolusionOrderItem>, VolusionOrderItem, VolusionDatabaseFunctionType>
		CreateDataAggregator(
			ReceivedDataListTimeDependentInfo<VolusionOrderItem> data,
			ICurrencyConvertor currencyConverter
		) {
			return new VolusionOrdersAgregator(data, currencyConverter);
		} // CreateDataAggregator
	} // class VolusionOrdersAggregatorFactory

	internal class VolusionOrdersAgregator
		: DataAggregatorBase<ReceivedDataListTimeDependentInfo<VolusionOrderItem>, VolusionOrderItem, VolusionDatabaseFunctionType>
	{
		public VolusionOrdersAgregator(
			ReceivedDataListTimeDependentInfo<VolusionOrderItem> orders, ICurrencyConvertor currencyConvertor
		): base(orders, currencyConvertor)
		{} // constructor

		private int GetShipedOrdersCount(IEnumerable<VolusionOrderItem> orders) {
			// TODO: update this once Channel Grabber implement order status.
			// Currently returns all the orders.
			return orders.Count();
		} // GetShippedOrdersCount

		private double GetAverageSumOfOrder(IEnumerable<VolusionOrderItem> orders) {
			double sum   = GetTotalSumOfOrders(orders);
			int    count = GetShipedOrdersCount(orders);

			return count == 0 ? 0 : sum / count;
		} // GetAverageSumOfOrders

		private double GetTotalSumOfOrders(IEnumerable<VolusionOrderItem> orders) {
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
			VolusionDatabaseFunctionType functionType,
			IEnumerable<VolusionOrderItem> orders
		) {
			switch (functionType) {
			case VolusionDatabaseFunctionType.AverageSumOfOrder:
				return GetAverageSumOfOrder(orders);

			case VolusionDatabaseFunctionType.NumOfOrders:
				return GetShipedOrdersCount(orders);

			case VolusionDatabaseFunctionType.TotalSumOfOrders:
				return GetTotalSumOfOrders(orders);
			
			default:
				throw new NotImplementedException();
			} // switch
		} // InternalCalculateAggregatorValue
	} // class VolusionOrdersAgregator
} // namespace