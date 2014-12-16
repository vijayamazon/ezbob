namespace EzBob.AmazonLib {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using AmazonDbLib;
	using CommonLib.TimePeriodLogic;

	/// <summary>
	/// This class is obsolete because aggregation was moved to DB (UpdateMpTotalsAmazon stored procedure).
	/// </summary>
	[Obsolete]
	internal class AmazonOrdersAgregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<AmazonOrderItem>, AmazonOrderItem, AmazonDatabaseFunctionType> {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<AmazonOrderItem>, AmazonOrderItem, AmazonDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<AmazonOrderItem> data, ICurrencyConvertor currencyConverter) {
			return new AmazonOrdersAgregator(data, currencyConverter);
		}
	}

	/// <summary>
	/// This class is obsolete because aggregation was moved to DB (UpdateMpTotalsAmazon stored procedure).
	/// </summary>
	[Obsolete]
	internal class AmazonOrdersAgregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<AmazonOrderItem>, AmazonOrderItem, AmazonDatabaseFunctionType> {
		public AmazonOrdersAgregator(ReceivedDataListTimeDependentInfo<AmazonOrderItem> orders, ICurrencyConvertor currencyConvertor)
			: base(orders, currencyConvertor) {
		}

		private int GetAverageItemsPerOrder(IEnumerable<AmazonOrderItem> orders) {
			var countItems = GetTotalItemsOrdered(orders);
			var countOrders = GetShipedOrdersCount(orders);

			return countOrders == 0 ? 0 : (int)Math.Round(countItems / (double)countOrders, MidpointRounding.AwayFromZero);
		}

		private int GetOrdersCount(IEnumerable<AmazonOrderItem> orders) {
			return orders.Count();
		}

		private int GetShipedOrdersCount(IEnumerable<AmazonOrderItem> orders) {
			return orders.Count(o => o.OrderStatus == AmazonOrdersList2ItemStatusType.Shipped);
		}

		private int GetCancelledOrdersCount(IEnumerable<AmazonOrderItem> orders) {
			return orders.Count(o => o.OrderStatus == AmazonOrdersList2ItemStatusType.Canceled);
		}

		private double GetAverageSumOfOrder(IEnumerable<AmazonOrderItem> orders) {
			var sum = GetTotalSumOfOrders(orders);
			var count = GetShipedOrdersCount(orders);

			return count == 0 ? 0 : sum / count;
		}

		private double GetOrdersCancellationRate(IEnumerable<AmazonOrderItem> orders) {
			var canceled = GetCancelledOrdersCount(orders);
			var other = GetOrdersCount(orders);

			return other == 0 ? 0 : canceled / (double)other;
		}

		private int GetTotalItemsOrdered(IEnumerable<AmazonOrderItem> orders) {
			return orders.Where(o => o.OrderStatus == AmazonOrdersList2ItemStatusType.Shipped && o.NumberOfItemsShipped != null).Sum(o => o.NumberOfItemsShipped.Value);
		}

		private double GetTotalSumOfOrders(IEnumerable<AmazonOrderItem> orders) {
			return orders.Where(o => o.OrderStatus == AmazonOrdersList2ItemStatusType.Shipped).Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.OrderTotal.CurrencyCode, o.OrderTotal.Value, o.PurchaseDate).Value);
		}

		private double GetTotalSumOfOrdersAnnualized(IEnumerable<AmazonOrderItem> orders) {
			var receivedDataListTimeDependentInfo = orders as ReceivedDataListTimeDependentInfo<AmazonOrderItem>;
			if (receivedDataListTimeDependentInfo == null) {
				return 0;
			}

			double totalSumOfOrders = GetTotalSumOfOrders(orders);
			return AnnualizeHelper.AnnualizeSum(receivedDataListTimeDependentInfo.TimePeriodType, receivedDataListTimeDependentInfo.SubmittedDate, totalSumOfOrders);
		}

		protected override object InternalCalculateAggregatorValue(AmazonDatabaseFunctionType functionType, IEnumerable<AmazonOrderItem> orders) {
			switch (functionType) {
			case AmazonDatabaseFunctionType.AverageItemsPerOrder:
				return GetAverageItemsPerOrder(orders);

			case AmazonDatabaseFunctionType.AverageSumOfOrder:
				return GetAverageSumOfOrder(orders);

			case AmazonDatabaseFunctionType.CancelledOrdersCount:
				return GetCancelledOrdersCount(orders);

			case AmazonDatabaseFunctionType.NumOfOrders:
				return GetShipedOrdersCount(orders);

			case AmazonDatabaseFunctionType.OrdersCancellationRate:
				return GetOrdersCancellationRate(orders);

			case AmazonDatabaseFunctionType.TotalItemsOrdered:
				return GetTotalItemsOrdered(orders);

			case AmazonDatabaseFunctionType.TotalSumOfOrders:
				return GetTotalSumOfOrders(orders);

			case AmazonDatabaseFunctionType.TotalSumOfOrdersAnnualized:
				return GetTotalSumOfOrdersAnnualized(orders);
			default:
				throw new NotImplementedException();
			}
		}
	}
}