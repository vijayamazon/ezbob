namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib.TimePeriodLogic;
	using log4net;

	internal class SageSalesInvoiceAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<SageSalesInvoice>, SageSalesInvoice, SageDatabaseFunctionType>
    {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<SageSalesInvoice>, SageSalesInvoice, SageDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<SageSalesInvoice> data, ICurrencyConvertor currencyConverter)
        {
			return new SageSalesInvoiceAggregator(data, currencyConverter);
        }
    }

	internal class SageSalesInvoiceAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<SageSalesInvoice>, SageSalesInvoice, SageDatabaseFunctionType>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SageSalesInvoiceAggregator));

		public SageSalesInvoiceAggregator(ReceivedDataListTimeDependentInfo<SageSalesInvoice> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
        }

		private int GetSalesInvoicesCount(IEnumerable<SageSalesInvoice> salesInvoices)
		{
			return salesInvoices.Count();
        }

		private double GetTotalSumOfSalesInvoices(IEnumerable<SageSalesInvoice> salesInvoices)
		{
			// consider handling currency conversion like:
			// return salesInvoices.Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.currency, (double)o.total_net_amount, o.date).Value);
			return salesInvoices.Sum(o => (double)o.total_net_amount);
		}

		protected override object InternalCalculateAggregatorValue(SageDatabaseFunctionType functionType, IEnumerable<SageSalesInvoice> invoices)
        {
            switch (functionType)
            {
                case SageDatabaseFunctionType.NumOfOrders:
					return GetSalesInvoicesCount(invoices);

				case SageDatabaseFunctionType.TotalSumOfOrders:
					return GetTotalSumOfSalesInvoices(invoices);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}