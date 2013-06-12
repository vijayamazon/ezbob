namespace FreeAgent
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib.TimePeriodLogic;
	using log4net;

	internal class FreeAgentInvoiceAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<FreeAgentInvoice>, FreeAgentInvoice, FreeAgentDatabaseFunctionType>
    {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<FreeAgentInvoice>, FreeAgentInvoice, FreeAgentDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<FreeAgentInvoice> data, ICurrencyConvertor currencyConverter)
        {
			return new FreeAgentInvoiceAggregator(data, currencyConverter);
        }
    }

	internal class FreeAgentInvoiceAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<FreeAgentInvoice>, FreeAgentInvoice, FreeAgentDatabaseFunctionType>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FreeAgentInvoiceAggregator));

		public FreeAgentInvoiceAggregator(ReceivedDataListTimeDependentInfo<FreeAgentInvoice> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
        }

		private int GetInvoicesCount(IEnumerable<FreeAgentInvoice> invoices)
		{
			return invoices.Count();
        }

		private double GetTotalSumOfInvoices(IEnumerable<FreeAgentInvoice> invoices)
		{
			return invoices.Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.currency, (double)o.net_value, o.dated_on).Value);
        }

		protected override object InternalCalculateAggregatorValue(FreeAgentDatabaseFunctionType functionType, IEnumerable<FreeAgentInvoice> invoices)
        {
            switch (functionType)
            {
                case FreeAgentDatabaseFunctionType.NumOfOrders:
					return GetInvoicesCount(invoices);

                case FreeAgentDatabaseFunctionType.TotalSumOfOrders:
					return GetTotalSumOfInvoices(invoices);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}