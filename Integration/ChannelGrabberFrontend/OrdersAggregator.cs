using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberFrontend {

	class OrdersAggregator
		: DataAggregatorBase<ReceivedDataListTimeDependentInfo<AInternalOrderItem>, AInternalOrderItem, FunctionType> {

		public OrdersAggregator(
			ReceivedDataListTimeDependentInfo<AInternalOrderItem> orders, ICurrencyConvertor currencyConvertor
		)
			: base(orders, currencyConvertor) {
		} // constructor

		protected override object InternalCalculateAggregatorValue(
			FunctionType functionType,
			IEnumerable<AInternalOrderItem> orders
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

			case FunctionType.TotalSumOfOrdersAnnualized:
				return GetTotalSumOfOrdersAnnualized(orders);

			default:
				throw new NotImplementedException();
			} // switch
		} // InternalCalculateAggregatorValue

		private int GetShipedOrdersCount(IEnumerable<AInternalOrderItem> orders) {
			return orders.Count(o => ChannelGrabberTypeName(o) == ChannelGrabberOrderItem.TypeName.Order);
		} // GetShippedOrdersCount

		private double GetAverageSumOfOrders(IEnumerable<AInternalOrderItem> orders) {
			int    count = GetShipedOrdersCount(orders);
			double sum   = count == 0 ? 0 : GetTotalSumOfOrders(orders);

			return count == 0 ? 0 : sum / count;
		} // GetAverageSumOfOrders

		private double GetTotalSumOfOrders(IEnumerable<AInternalOrderItem> orders) {
			return orders
				.Where(o => ChannelGrabberTypeName(o) == ChannelGrabberOrderItem.TypeName.Order)
				.Sum(o => ChannelGrabberConvert(o));
		} // GetTotalSumOfOrders

		private double GetTotalSumOfOrdersAnnualized(IEnumerable<AInternalOrderItem> orders) {
			var ordersWithExtraInfo = orders as ReceivedDataListTimeDependentInfo<AInternalOrderItem>;
			if (ordersWithExtraInfo == null) {
				return 0;
			}

			double sum = GetTotalSumOfOrders(orders);
			return AnnualizeHelper.AnnualizeSum(ordersWithExtraInfo.TimePeriodType, ordersWithExtraInfo.SubmittedDate, sum);
		} // GetTotalSumOfOrdersAnnualized

		private int GetExpensesCount(IEnumerable<AInternalOrderItem> orders) {
			return orders.Count(o => ChannelGrabberTypeName(o) == ChannelGrabberOrderItem.TypeName.Expense);
		} // GetExpensesCount

		private double GetAverageSumOfExpenses(IEnumerable<AInternalOrderItem> orders) {
			int    count = GetExpensesCount(orders);
			double sum   = count == 0 ? 0 : GetTotalSumOfExpenses(orders);

			return count == 0 ? 0 : sum / count;
		} // GetAverageSumOfExpenses

		private double GetTotalSumOfExpenses(IEnumerable<AInternalOrderItem> orders) {
			return orders
				.Where(o => ChannelGrabberTypeName(o) == ChannelGrabberOrderItem.TypeName.Expense)
				.Sum(o => ChannelGrabberConvert(o));
		} // GetTotalSumOfExpenses

		private ChannelGrabberOrderItem.TypeName ChannelGrabberTypeName(AInternalOrderItem o) {
			if (!(o is ChannelGrabberOrderItem))
				return ChannelGrabberOrderItem.TypeName.Other;

			var x = (ChannelGrabberOrderItem)o;

			return x.GetTypeName();
		} // ChannelGrabberTypeName

		private double ChannelGrabberConvert(AInternalOrderItem o) {
			if (!(o is ChannelGrabberOrderItem))
				return 0;

			var x = (ChannelGrabberOrderItem)o;

			return x.Convert(CurrencyConverter);
		} // ChannelGrabberConvert

	} // class OrdersAggregator

} // namespace
