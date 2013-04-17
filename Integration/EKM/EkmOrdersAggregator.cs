using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EzBob.CommonLib.TimePeriodLogic;
using log4net;

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
        private static readonly ILog Log = LogManager.GetLogger(typeof(EkmOrdersAggregator));
        private readonly string[] _ekmCompleteStatusList = new string[]
            {
                "dispatched",
                "processing",
                "pending",
                "complete",
            };

        private readonly string[] _ekmCancelledStatusList = new string[]
            {
                "refunded",
                "failed",
                "cancelled"
            };

        public EkmOrdersAggregator(ReceivedDataListTimeDependentInfo<EkmOrderItem> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
            LogOtherStatuses(orders);
        }

        private int GetOrdersCount(IEnumerable<EkmOrderItem> orders)
        {
            return orders.Count(o => !_ekmCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower()));
        }

        private int GetCancelledOrdersCount(IEnumerable<EkmOrderItem> orders)
        {
            return orders.Count(o => _ekmCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower()));
        }

        private int GetOtherOrdersCount(IEnumerable<EkmOrderItem> orders)
        {
            return orders.Count(o =>
               !_ekmCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower()) &&
               !_ekmCompleteStatusList.Contains(o.OrderStatus.Trim().ToLower())
             );
        }

        private double GetTotalSumOfOrders(IEnumerable<EkmOrderItem> orders)
        {
            return orders.Where(o => o.TotalCost.HasValue && !_ekmCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower())).
                Sum(o => (double)o.TotalCost /*CurrencyConverter.ConvertToBaseCurrency(o.OrderTotal.CurrencyCode, o.OrderTotal.Value, o.PurchaseDate).Value*/);
        }

        private double GetTotalSumOfCancelledOrders(IEnumerable<EkmOrderItem> orders)
        {
            return orders.Where(o => o.TotalCost.HasValue && _ekmCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower())).
                Sum(o => (double)o.TotalCost /*CurrencyConverter.ConvertToBaseCurrency(o.OrderTotal.CurrencyCode, o.OrderTotal.Value, o.PurchaseDate).Value*/);
        }

        private double GetTotalSumOfOtherOrders(IEnumerable<EkmOrderItem> orders)
        {
            return orders.Where(o => o.TotalCost.HasValue &&
                !_ekmCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower()) &&
                !_ekmCompleteStatusList.Contains(o.OrderStatus.Trim().ToLower())).
                Sum(o => (double)o.TotalCost /*CurrencyConverter.ConvertToBaseCurrency(o.OrderTotal.CurrencyCode, o.OrderTotal.Value, o.PurchaseDate).Value*/);
        }

        private double GetAverageSumOfOrder(IEnumerable<EkmOrderItem> orders)
        {
            double sum = GetTotalSumOfOrders(orders);
            var count = GetOrdersCount(orders);

            return count == 0 ? 0 : sum / count;
        }

        private double GetAverageSumOfCancelledOrder(IEnumerable<EkmOrderItem> orders)
        {
            double sum = GetTotalSumOfCancelledOrders(orders);
            var count = GetCancelledOrdersCount(orders);

            return count == 0 ? 0 : sum / count;
        }

        private double GetAverageSumOfOtherOrder(IEnumerable<EkmOrderItem> orders)
        {
            double sum = GetTotalSumOfOtherOrders(orders);
            var count = GetOtherOrdersCount(orders);

            return count == 0 ? 0 : sum / count;
        }

        private void LogOtherStatuses(IEnumerable<EkmOrderItem> orders)
        {
            IEnumerable<string> otherStatuses = orders.
                Where(o => !_ekmCancelledStatusList.Contains(o.OrderStatus.Trim().ToLower()) &&
                           !_ekmCompleteStatusList.Contains(o.OrderStatus.Trim().ToLower())).
                OrderBy(o => o.OrderStatus).
                Select(o => o.OrderStatus.Trim()).
                Distinct();

            foreach (var otherStatus in otherStatuses)
            {
                Log.InfoFormat("Ekm other status detected: |{0}|", otherStatus);
            }
        }

        protected override object InternalCalculateAggregatorValue(EkmDatabaseFunctionType functionType, IEnumerable<EkmOrderItem> orders)
        {
            switch (functionType)
            {
                case EkmDatabaseFunctionType.NumOfOrders:
                    return GetOrdersCount(orders);

                case EkmDatabaseFunctionType.AverageSumOfOrder:
                    return GetAverageSumOfOrder(orders);

                case EkmDatabaseFunctionType.TotalSumOfOrders:
                    return GetTotalSumOfOrders(orders);

                case EkmDatabaseFunctionType.NumOfCancelledOrders:
                    return GetCancelledOrdersCount(orders);

                case EkmDatabaseFunctionType.AverageSumOfCancelledOrder:
                    return GetAverageSumOfCancelledOrder(orders);

                case EkmDatabaseFunctionType.TotalSumOfCancelledOrders:
                    return GetAverageSumOfCancelledOrder(orders);

                case EkmDatabaseFunctionType.NumOfOtherOrders:
                    return GetAverageSumOfOtherOrder(orders);

                case EkmDatabaseFunctionType.AverageSumOfOtherOrder:
                    return GetAverageSumOfOtherOrder(orders);

                case EkmDatabaseFunctionType.TotalSumOfOtherOrders:
                    return GetAverageSumOfOtherOrder(orders);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}