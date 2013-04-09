using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;

namespace EKM
{
    internal class EkmOrdersAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<EkmOrderItem>, EkmOrderItem, EkmDatabaseFunctionType>
    {
        public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<EkmOrderItem>, EkmOrderItem, EkmDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<EkmOrderItem> data, ICurrencyConvertor currencyConverter)
        {
            return new EkmOrdersAggregator(data, currencyConverter);
        }
    }

    internal class EkmOrdersAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<EkmOrderItem>, EkmOrderItem, EkmDatabaseFunctionType>
    {
        public EkmOrdersAggregator(ReceivedDataListTimeDependentInfo<EkmOrderItem> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
        }

        private int GetOrdersCount(IEnumerable<EkmOrderItem> orders)
        {
            return orders.Count();
        }

        private int GetShipedOrdersCount(IEnumerable<EkmOrderItem> orders)
        {
            //TODO check status field!!!
            return orders.Count();//o => o.OrderStatus == EkmOrdersList2ItemStatusType.Shipped);
        }

        private double GetAverageSumOfOrder(IEnumerable<EkmOrderItem> orders)
        {
            var sum = GetTotalSumOfOrders(orders);
            var count = GetShipedOrdersCount(orders);

            return count == 0 ? 0 : sum / count;
        }
       
        private double GetTotalSumOfOrders(IEnumerable<EkmOrderItem> orders)
        {
            //TODO check status field!!!
            return orders./*Where(o => o.OrderStatus == EkmOrdersList2ItemStatusType.Shipped).*/Sum(o => (double)o.TotalCost /*CurrencyConverter.ConvertToBaseCurrency(o.OrderTotal.CurrencyCode, o.OrderTotal.Value, o.PurchaseDate).Value*/);
        }

        protected override object InternalCalculateAggregatorValue(EkmDatabaseFunctionType functionType, IEnumerable<EkmOrderItem> orders)
        {
            switch (functionType)
            {
                case EkmDatabaseFunctionType.AverageSumOfOrder:
                    return GetAverageSumOfOrder(orders);

                case EkmDatabaseFunctionType.NumOfOrders:
                    return GetShipedOrdersCount(orders);

                case EkmDatabaseFunctionType.TotalSumOfOrders:
                    return GetTotalSumOfOrders(orders);
                
                default:
                    throw new NotImplementedException();
            }
        }
    }
}