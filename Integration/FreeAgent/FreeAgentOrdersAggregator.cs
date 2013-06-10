namespace FreeAgent
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib.TimePeriodLogic;
	using log4net;

	internal class FreeAgentInvoicesAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<FreeAgentInvoice>, FreeAgentInvoice, FreeAgentDatabaseFunctionType>
    {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<FreeAgentInvoice>, FreeAgentInvoice, FreeAgentDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<FreeAgentInvoice> data, ICurrencyConvertor currencyConverter)
        {
			return new FreeAgentInvoicesAggregator(data, currencyConverter);
        }
    }

	internal class FreeAgentInvoicesAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<FreeAgentInvoice>, FreeAgentInvoice, FreeAgentDatabaseFunctionType>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FreeAgentInvoicesAggregator));

		public FreeAgentInvoicesAggregator(ReceivedDataListTimeDependentInfo<FreeAgentInvoice> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
        }

		private int GetInvoicesCount(IEnumerable<FreeAgentInvoice> invoice)
		{
			return invoice.Count();
        }

		private double GetTotalSumOfInvoices(IEnumerable<FreeAgentInvoice> invoice)
		{
			return (double)invoice.Sum(o => o.net_value);
        }

		protected override object InternalCalculateAggregatorValue(FreeAgentDatabaseFunctionType functionType, IEnumerable<FreeAgentInvoice> orders)
        {
            switch (functionType)
            {
                case FreeAgentDatabaseFunctionType.NumOfOrders:
					return GetInvoicesCount(orders);

                case FreeAgentDatabaseFunctionType.TotalSumOfOrders:
					return GetTotalSumOfInvoices(orders);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}