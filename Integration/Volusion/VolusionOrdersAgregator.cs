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
        {}

        private int GetOrdersCount(IEnumerable<VolusionOrderItem> orders) {
            return orders.Count();
        } // GetOrdersCount

        private int GetShipedOrdersCount(IEnumerable<VolusionOrderItem> orders) {
            //TODO check status field
            return orders.Count();//o => o.OrderStatus == VolusionOrdersList2ItemStatusType.Shipped);
        } // GetShippedOrdersCount

        private double GetAverageSumOfOrder(IEnumerable<VolusionOrderItem> orders) {
            var sum = GetTotalSumOfOrders(orders);
            var count = GetShipedOrdersCount(orders);

            return count == 0 ? 0 : sum / count;
        } // GetAverageSumOfOrders
       
        private double GetTotalSumOfOrders(IEnumerable<VolusionOrderItem> orders) {
            // TODO: check status field
            // return orders.Where(o => o.OrderStatus == VolusionOrdersList2ItemStatusType.Shipped).Sum(o => (double)o.TotalCost /*CurrencyConverter.ConvertToBaseCurrency(o.OrderTotal.CurrencyCode, o.OrderTotal.Value, o.PurchaseDate).Value*/);
	        return orders.Sum(o => (double)o.TotalCost);
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