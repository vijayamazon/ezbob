using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;

namespace PayPoint
{
    internal class PayPointOrdersAgregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<PayPointOrderItem>, PayPointOrderItem, PayPointDatabaseFunctionType>
    {
        public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<PayPointOrderItem>, PayPointOrderItem, PayPointDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<PayPointOrderItem> data, ICurrencyConvertor currencyConverter)
        {
            return new PayPointOrdersAgregator(data, currencyConverter);
        }
    }

    internal class PayPointOrdersAgregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<PayPointOrderItem>, PayPointOrderItem, PayPointDatabaseFunctionType>
    {
        public PayPointOrdersAgregator(ReceivedDataListTimeDependentInfo<PayPointOrderItem> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
        }
        
        private int GetOrdersCount(IEnumerable<PayPointOrderItem> orders)
        {
            return orders.Count();
        }

        private int GetShipedOrdersCount(IEnumerable<PayPointOrderItem> orders)
        {
            //TODO check status field
            return orders.Count();//o => o.OrderStatus == PayPointOrdersList2ItemStatusType.Shipped);
        }

        /*
         private double GetAverageSumOfOrder(IEnumerable<PayPointOrderItem> orders)
         {
             var sum = GetTotalSumOfOrders(orders);
             var count = GetShipedOrdersCount(orders);

             return count == 0 ? 0 : sum / count;
         }
         private double GetTotalSumOfOrders(IEnumerable<PayPointOrderItem> orders)
         {
             //TODO check status field
             return orders.Sum(o => (double)o.TotalCost );
         }
         */
        protected override object InternalCalculateAggregatorValue(PayPointDatabaseFunctionType functionType, IEnumerable<PayPointOrderItem> orders)
        {
            switch (functionType)
            {
                case PayPointDatabaseFunctionType.BLAH:
                    return null;
                
                default:
                    throw new NotImplementedException();
            }
        }
    }
}