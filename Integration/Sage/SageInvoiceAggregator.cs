namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib.TimePeriodLogic;
	using log4net;

	internal class SageInvoiceAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<SageInvoice>, SageInvoice, SageDatabaseFunctionType>
    {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<SageInvoice>, SageInvoice, SageDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<SageInvoice> data, ICurrencyConvertor currencyConverter)
        {
			return new SageInvoiceAggregator(data, currencyConverter);
        }
    }

	internal class SageInvoiceAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<SageInvoice>, SageInvoice, SageDatabaseFunctionType>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SageInvoiceAggregator));

		public SageInvoiceAggregator(ReceivedDataListTimeDependentInfo<SageInvoice> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
        }

		private int GetInvoicesCount(IEnumerable<SageInvoice> invoices)
		{
			return invoices.Count();
        }

		private double GetTotalSumOfInvoices(IEnumerable<SageInvoice> invoices)
		{
			// consider handling currency conversion like:
			// return invoices.Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.currency, (double)o.total_net_amount, o.date).Value);
			return invoices.Sum(o => (double)o.total_net_amount);
		}

		protected override object InternalCalculateAggregatorValue(SageDatabaseFunctionType functionType, IEnumerable<SageInvoice> invoices)
        {
            switch (functionType)
            {
                case SageDatabaseFunctionType.NumOfOrders:
					return GetInvoicesCount(invoices);

				case SageDatabaseFunctionType.TotalSumOfOrders:
					return GetTotalSumOfInvoices(invoices);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}