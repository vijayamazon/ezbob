namespace PayPoint
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.DatabaseWrapper.Order;
    using EzBob.CommonLib.TimePeriodLogic;

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

        protected override object InternalCalculateAggregatorValue(PayPointDatabaseFunctionType functionType, IEnumerable<PayPointOrderItem> orders)
        {
            switch (functionType)
            {
                case PayPointDatabaseFunctionType.NumOfOrders:
                    return GetOrdersCount(orders);
                
                default:
                    throw new NotImplementedException();
            }
        }
    }
}