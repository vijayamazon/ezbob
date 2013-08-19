using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberFrontend {
	public class OrdersAggregatorFactory
		: DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<InternalOrderItem>, InternalOrderItem, FunctionType>
	{
		public override
			DataAggregatorBase<ReceivedDataListTimeDependentInfo<InternalOrderItem>, InternalOrderItem, FunctionType>
		CreateDataAggregator(
			ReceivedDataListTimeDependentInfo<InternalOrderItem> data,
			ICurrencyConvertor currencyConverter
		) {
			return new OrdersAgregator(data, currencyConverter);
		} // CreateDataAggregator
	} // class OrdersAggregatorFactory

	internal class OrdersAgregator
		: DataAggregatorBase<ReceivedDataListTimeDependentInfo<InternalOrderItem>, InternalOrderItem, FunctionType>
	{
		public OrdersAgregator(
			ReceivedDataListTimeDependentInfo<InternalOrderItem> orders, ICurrencyConvertor currencyConvertor
		) : base(orders, currencyConvertor) {
		} // constructor

		private int GetShipedOrdersCount(IEnumerable<InternalOrderItem> orders) {
			return orders.Count(o => (o.IsExpense == 0)); // && (o.OrderStatus == "Dispatched"));
		} // GetShippedOrdersCount

		private double GetAverageSumOfOrders(IEnumerable<InternalOrderItem> orders) {
			int    count = GetShipedOrdersCount(orders);
			double sum   = count == 0 ? 0 : GetTotalSumOfOrders(orders);

			return count == 0 ? 0 : sum / count;
		} // GetAverageSumOfOrders

		private double GetTotalSumOfOrders(IEnumerable<InternalOrderItem> orders) {
			return orders
				.Where(o => (o.IsExpense == 0) && o.TotalCost.HasValue) // && (o.OrderStatus == "Dispatched"))
				.Sum(
					o =>
					CurrencyConverter.ConvertToBaseCurrency(
						o.CurrencyCode,
						(double)o.TotalCost,
						o.PurchaseDate
					).Value
				);
		} // GetTotalSumOfOrders

		private int GetExpensesCount(IEnumerable<InternalOrderItem> orders) {
			return orders.Count(o => o.IsExpense == 1);
		} // GetExpensesCount

		private double GetAverageSumOfExpenses(IEnumerable<InternalOrderItem> orders) {
			int    count = GetExpensesCount(orders);
			double sum   = count == 0 ? 0 : GetTotalSumOfExpenses(orders);

			return count == 0 ? 0 : sum / count;
		} // GetAverageSumOfExpenses

		private double GetTotalSumOfExpenses(IEnumerable<InternalOrderItem> orders) {
			return orders
				.Where(o => (o.IsExpense == 1) && o.TotalCost.HasValue)
				.Sum(
					o =>
					CurrencyConverter.ConvertToBaseCurrency(
						o.CurrencyCode,
						(double)o.TotalCost,
						o.PurchaseDate
					).Value
				);
		} // GetTotalSumOfExpenses

		protected override object InternalCalculateAggregatorValue(
			FunctionType functionType,
			IEnumerable<InternalOrderItem> orders
		) {
			switch (functionType) {
			case FunctionType.NumOfOrders:
				return GetShipedOrdersCount(orders);

			case FunctionType.AverageSumOfOrders:
				return GetAverageSumOfOrders(orders);

			case FunctionType.TotalSumOfOrders:
				return GetTotalSumOfOrders(orders);

			case FunctionType.NumOfExpenses:
				return GetExpensesCount(orders);
			
			case FunctionType.AverageSumOfExpenses:
				return GetAverageSumOfExpenses(orders);
			
			case FunctionType.TotalSumOfExpenses:
				return GetTotalSumOfExpenses(orders);
			
			default:
				throw new NotImplementedException();
			} // switch
		} // InternalCalculateAggregatorValue
	} // class OrdersAgregator
} // namespace