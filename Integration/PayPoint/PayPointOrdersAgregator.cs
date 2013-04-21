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

        private IEnumerable<PayPointOrderItem> GetAuthorisedOrders(IEnumerable<PayPointOrderItem> orders)
        {
            return orders.Where(a => a.status == "Authorised");
        }

        private int GetOrdersCount(IEnumerable<PayPointOrderItem> orders)
        {
            return orders.Count();
        }

        private double GetAuthorisedOrdersSum(IEnumerable<PayPointOrderItem> orders)
        {
            return (double)GetAuthorisedOrders(orders).Sum(payPointOrderItem => payPointOrderItem.amount);
        }

        private double GetOrdersAverage(IEnumerable<PayPointOrderItem> orders)
        {
            var authorisedOrders = GetAuthorisedOrders(orders);
            decimal sum = authorisedOrders.Sum(payPointOrderItem => payPointOrderItem.amount);
            return (double)(sum / authorisedOrders.Count());
        }

        private int GetNumOfFailures(IEnumerable<PayPointOrderItem> orders)
        {
            return orders.Count(a => a.status != "Authorised");
        }

        private double GetCancellationRate(IEnumerable<PayPointOrderItem> orders)
        {
            var authorisedOrders = GetAuthorisedOrders(orders);
            return (authorisedOrders.Count() * 100 / GetOrdersCount(orders));
        }

        private double GetCancellationValue(IEnumerable<PayPointOrderItem> orders)
        {
            decimal sumOfUnAuthorisedOrders = orders.Where(a => a.status != "Authorised").Sum(o => o.amount);
            decimal sunOfAllOrders = orders.Sum(o => o.amount);
            return (double)(sumOfUnAuthorisedOrders * 100 / sunOfAllOrders);
        }
        
        protected override object InternalCalculateAggregatorValue(PayPointDatabaseFunctionType functionType, IEnumerable<PayPointOrderItem> orders)
        {
            switch (functionType)
            {
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
    }
}