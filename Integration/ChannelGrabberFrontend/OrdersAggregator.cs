using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberFrontend {
	#region class OrdersAggregator

	class OrdersAggregator
		: DataAggregatorBase<ReceivedDataListTimeDependentInfo<AInternalOrderItem>, AInternalOrderItem, FunctionType>
	{
		#region public

		#region constructor

		public OrdersAggregator(
			ReceivedDataListTimeDependentInfo<AInternalOrderItem> orders, ICurrencyConvertor currencyConvertor
		) : base(orders, currencyConvertor) {
		} // constructor

		#endregion constructor

		#endregion public

		#region protected

		#region method InternalCalculateAggregatorValue

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

		#endregion method InternalCalculateAggregatorValue

		#endregion protected

		#region private

		#region aggregation functions implementation

		#region orders

		#region method GetShipedOrdersCount

		private int GetShipedOrdersCount(IEnumerable<AInternalOrderItem> orders) {
			return orders.Count(o => ChannelGrabberTypeName(o) == ChannelGrabberOrderItem.TypeName.Order);
		} // GetShippedOrdersCount

		#endregion method GetShipedOrdersCount

		#region method GetAverageSumOfOrders

		private double GetAverageSumOfOrders(IEnumerable<AInternalOrderItem> orders) {
			int    count = GetShipedOrdersCount(orders);
			double sum   = count == 0 ? 0 : GetTotalSumOfOrders(orders);

			return count == 0 ? 0 : sum / count;
		} // GetAverageSumOfOrders

		#endregion method GetAverageSumOfOrders

		#region method GetTotalSumOfOrders

		private double GetTotalSumOfOrders(IEnumerable<AInternalOrderItem> orders)
		{
			return orders
				.Where(o => ChannelGrabberTypeName(o) == ChannelGrabberOrderItem.TypeName.Order)
				.Sum(o => ChannelGrabberConvert(o));
		} // GetTotalSumOfOrders

		#endregion method GetTotalSumOfOrders

		#region method GetTotalSumOfOrdersAnnualized

		private double GetTotalSumOfOrdersAnnualized(IEnumerable<AInternalOrderItem> orders)
		{
			var ordersWithExtraInfo = orders as ReceivedDataListTimeDependentInfo<AInternalOrderItem>;
			if (ordersWithExtraInfo == null)
			{
				return 0;
			}

			double sum = GetTotalSumOfOrders(orders);
			return AnnualizeHelper.AnnualizeSum(ordersWithExtraInfo.TimePeriodType, ordersWithExtraInfo.SubmittedDate, sum);
		} // GetTotalSumOfOrdersAnnualized

		#endregion method GetTotalSumOfOrdersAnnualized

		#endregion orders

		#region expenses

		#region method GetExpensesCount

		private int GetExpensesCount(IEnumerable<AInternalOrderItem> orders) {
			return orders.Count(o => ChannelGrabberTypeName(o) == ChannelGrabberOrderItem.TypeName.Expense);
		} // GetExpensesCount

		#endregion method GetExpensesCount

		#region method GetAverageSumOfExpenses

		private double GetAverageSumOfExpenses(IEnumerable<AInternalOrderItem> orders) {
			int    count = GetExpensesCount(orders);
			double sum   = count == 0 ? 0 : GetTotalSumOfExpenses(orders);

			return count == 0 ? 0 : sum / count;
		} // GetAverageSumOfExpenses

		#endregion method GetAverageSumOfExpenses

		#region method GetTotalSumOfExpenses

		private double GetTotalSumOfExpenses(IEnumerable<AInternalOrderItem> orders) {
			return orders
				.Where(o => ChannelGrabberTypeName(o) == ChannelGrabberOrderItem.TypeName.Expense)
				.Sum(o => ChannelGrabberConvert(o));
		} // GetTotalSumOfExpenses

		#endregion method GetTotalSumOfExpenses

		#endregion expenses

		#endregion aggregation functions implementation

		#region Channel Grabber Flavour helpers

		#region method ChannelGrabberTypeName

		private ChannelGrabberOrderItem.TypeName ChannelGrabberTypeName(AInternalOrderItem o) {
			if (!(o is ChannelGrabberOrderItem))
				return ChannelGrabberOrderItem.TypeName.Other;

			var x = (ChannelGrabberOrderItem)o;

			return x.GetTypeName();
		} // ChannelGrabberTypeName

		#endregion method ChannelGrabberTypeName

		#region method ChannelGrabberConvert

		private double ChannelGrabberConvert(AInternalOrderItem o) {
			if (!(o is ChannelGrabberOrderItem))
				return 0;

			var x = (ChannelGrabberOrderItem)o;

			return x.Convert(CurrencyConverter);
		} // ChannelGrabberConvert

		#endregion method ChannelGrabberConvert

		#endregion Channel Grabber Flavour helpers

		#endregion private
	} // class OrdersAggregator

	#endregion class OrdersAggregator
} // namespace