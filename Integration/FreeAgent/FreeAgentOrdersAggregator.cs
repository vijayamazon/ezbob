namespace FreeAgent
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib.TimePeriodLogic;
	using log4net;

	internal class FreeAgentOrdersAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<FreeAgentOrderItem>, FreeAgentOrderItem, FreeAgentDatabaseFunctionType>
    {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<FreeAgentOrderItem>, FreeAgentOrderItem, FreeAgentDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<FreeAgentOrderItem> data, ICurrencyConvertor currencyConverter)
        {
			return new FreeAgentOrdersAggregator(data, currencyConverter);
        }
    }

    internal class FreeAgentOrdersAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<FreeAgentOrderItem>, FreeAgentOrderItem, FreeAgentDatabaseFunctionType>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FreeAgentOrdersAggregator));
      
		public FreeAgentOrdersAggregator(ReceivedDataListTimeDependentInfo<FreeAgentOrderItem> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
        }

		private int GetOrdersCount(IEnumerable<FreeAgentOrderItem> orders)
		{
			return orders.Count();
        }

		private double GetTotalSumOfOrders(IEnumerable<FreeAgentOrderItem> orders)
		{
			// should sum according to net_value?
			// convert currency here?
			return (double)orders.Sum(o => o.net_value);
        }

        protected override object InternalCalculateAggregatorValue(FreeAgentDatabaseFunctionType functionType, IEnumerable<FreeAgentOrderItem> orders)
        {
            switch (functionType)
            {
                case FreeAgentDatabaseFunctionType.NumOfOrders:
                    return GetOrdersCount(orders);

                case FreeAgentDatabaseFunctionType.TotalSumOfOrders:
                    return GetTotalSumOfOrders(orders);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}