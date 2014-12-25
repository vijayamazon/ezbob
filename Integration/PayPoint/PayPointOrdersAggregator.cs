/*
 * This file is kept for a very unlikely case we will need PayPoint aggregations.
 * Currently (December 25 2014, Merry Xmas!) this connector is not used because PayPoint forbid it.
 * All the other connectors were transformed from such "code style" into "SQL style" (UpdateMpTotals***
 * stored procedures).
 *
namespace PayPoint {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib.TimePeriodLogic;

	internal class PayPointOrdersAggregator :
		DataAggregatorBase<
			ReceivedDataListTimeDependentInfo<PayPointOrderItem>,
			PayPointOrderItem,
			PayPointDatabaseFunctionType
		> {
		public PayPointOrdersAggregator(
			ReceivedDataListTimeDependentInfo<PayPointOrderItem> orders,
			ICurrencyConvertor currencyConvertor
			)
			: base(orders, currencyConvertor) {
		}

		protected override object InternalCalculateAggregatorValue(
			PayPointDatabaseFunctionType functionType,
			IEnumerable<PayPointOrderItem> orders
		) {
			switch (functionType) {
			case PayPointDatabaseFunctionType.NumOfOrders:
				return GetOrdersCount(orders);

			case PayPointDatabaseFunctionType.SumOfAuthorisedOrders:
				return GetAuthorisedOrdersSum(orders);

			case PayPointDatabaseFunctionType.OrdersAverage:
				return GetOrdersAverage(orders);

			case PayPointDatabaseFunctionType.NumOfFailures:
				return GetNumOfFailures(orders);

			case PayPointDatabaseFunctionType.CancellationRate:
				return GetCancellationRate(orders);

			case PayPointDatabaseFunctionType.CancellationValue:
				return GetCancellationValue(orders);

			default:
				throw new NotImplementedException();
			}
		}

		private IEnumerable<PayPointOrderItem> GetAuthorisedOrders(IEnumerable<PayPointOrderItem> orders) {
			return orders.Where(a => a.status == "Authorised");
		}

		private double GetAuthorisedOrdersSum(IEnumerable<PayPointOrderItem> orders) {
			return (double)GetAuthorisedOrders(orders).Sum(payPointOrderItem => payPointOrderItem.amount);
		}

		private double GetCancellationRate(IEnumerable<PayPointOrderItem> orders) {
			return (double)GetNumOfFailures(orders) / (double)GetOrdersCount(orders);
		}

		private double GetCancellationValue(IEnumerable<PayPointOrderItem> orders) {
			decimal sumOfUnAuthorisedOrders = orders.Where(a => a.status != "Authorised").Sum(o => o.amount);
			decimal sumOfAllOrders = orders.Sum(o => o.amount);
			return (double)(sumOfUnAuthorisedOrders / sumOfAllOrders);
		}

		private int GetNumOfFailures(IEnumerable<PayPointOrderItem> orders) {
			return orders.Count(a => a.status != "Authorised");
		}

		private double GetOrdersAverage(IEnumerable<PayPointOrderItem> orders) {
			var authorisedOrders = GetAuthorisedOrders(orders);
			decimal sum = authorisedOrders.Sum(payPointOrderItem => payPointOrderItem.amount);
			return (double)(sum / authorisedOrders.Count());
		}

		private int GetOrdersCount(IEnumerable<PayPointOrderItem> orders) {
			return orders.Count();
		}
	}
}
*/
