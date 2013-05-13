namespace YodleeLib.connector
{
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib.TimePeriodLogic;
	using log4net;

    internal class YodleeOrdersAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<YodleeOrderItem>, YodleeOrderItem, YodleeDatabaseFunctionType>
    {
        public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<YodleeOrderItem>, YodleeOrderItem, YodleeDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<YodleeOrderItem> data, ICurrencyConvertor currencyConverter)
        {
            return new YodleeOrdersAggregator(data, currencyConverter);
        }
    }

    internal class YodleeOrdersAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<YodleeOrderItem>, YodleeOrderItem, YodleeDatabaseFunctionType>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeOrdersAggregator));
       
        public YodleeOrdersAggregator(ReceivedDataListTimeDependentInfo<YodleeOrderItem> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
           
        }

        //private int GetOrdersCount(IEnumerable<YodleeOrderItem> orders)
        //{
        //    return orders.Count(o => !_YodleeCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower()));
        //}

        //private int GetCancelledOrdersCount(IEnumerable<YodleeOrderItem> orders)
        //{
        //    return orders.Count(o => _YodleeCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower()));
        //}

        //private int GetOtherOrdersCount(IEnumerable<YodleeOrderItem> orders)
        //{
        //    return orders.Count(o =>
        //       !_YodleeCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower()) &&
        //       !_YodleeCompleteStatusList.Contains(o.OrderStatus.Trim().ToLower())
        //     );
        //}

        //private double GetTotalSumOfOrders(IEnumerable<YodleeOrderItem> orders)
        //{
        //    return orders.Where(o => o.TotalCost.HasValue && !_YodleeCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower())).
        //        Sum(o => (double)o.TotalCost /*CurrencyConverter.ConvertToBaseCurrency(o.OrderTotal.CurrencyCode, o.OrderTotal.Value, o.PurchaseDate).Value*/);
        //}

        //private double GetTotalSumOfCancelledOrders(IEnumerable<YodleeOrderItem> orders)
        //{
        //    return orders.Where(o => o.TotalCost.HasValue && _YodleeCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower())).
        //        Sum(o => (double)o.TotalCost /*CurrencyConverter.ConvertToBaseCurrency(o.OrderTotal.CurrencyCode, o.OrderTotal.Value, o.PurchaseDate).Value*/);
        //}

        //private double GetTotalSumOfOtherOrders(IEnumerable<YodleeOrderItem> orders)
        //{
        //    return orders.Where(o => o.TotalCost.HasValue &&
        //        !_YodleeCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower()) &&
        //        !_YodleeCompleteStatusList.Contains(o.OrderStatus.Trim().ToLower())).
        //        Sum(o => (double)o.TotalCost /*CurrencyConverter.ConvertToBaseCurrency(o.OrderTotal.CurrencyCode, o.OrderTotal.Value, o.PurchaseDate).Value*/);
        //}

        //private double GetAverageSumOfOrder(IEnumerable<YodleeOrderItem> orders)
        //{
        //    double sum = GetTotalSumOfOrders(orders);
        //    var count = GetOrdersCount(orders);

        //    return count == 0 ? 0 : sum / count;
        //}

        //private double GetAverageSumOfCancelledOrder(IEnumerable<YodleeOrderItem> orders)
        //{
        //    double sum = GetTotalSumOfCancelledOrders(orders);
        //    var count = GetCancelledOrdersCount(orders);

        //    return count == 0 ? 0 : sum / count;
        //}

        //private double GetAverageSumOfOtherOrder(IEnumerable<YodleeOrderItem> orders)
        //{
        //    double sum = GetTotalSumOfOtherOrders(orders);
        //    var count = GetOtherOrdersCount(orders);

        //    return count == 0 ? 0 : sum / count;
        //}

        //private double GetOrdersCancellationRate(IEnumerable<YodleeOrderItem> orders)
        //{
        //    var canceled = GetCancelledOrdersCount(orders);
        //    var other = GetOrdersCount(orders);

        //    return other == 0 ? 0 : canceled / (double)other;
        //}

        protected override object InternalCalculateAggregatorValue(YodleeDatabaseFunctionType functionType, IEnumerable<YodleeOrderItem> orders)
        {
            switch (functionType)
            {

                case YodleeDatabaseFunctionType.TotlaIncome:
                    return GetTotlaIncome(orders);

                case YodleeDatabaseFunctionType.TotalExpense:
                    return GetTotalExpense(orders);
                
                case YodleeDatabaseFunctionType.CurrentBalance:
                    return GetCurrentBalance(orders);

                default:
                    throw new NotImplementedException();
            }
        }

        private object GetCurrentBalance(IEnumerable<YodleeOrderItem> orders)
        {
            throw new NotImplementedException();
        }

        private object GetTotalExpense(IEnumerable<YodleeOrderItem> orders)
        {
            throw new NotImplementedException();
        }

        private object GetTotlaIncome(IEnumerable<YodleeOrderItem> orders)
        {
            throw new NotImplementedException();
        }
    }
}