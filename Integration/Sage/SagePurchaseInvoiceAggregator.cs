namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib.TimePeriodLogic;
	using log4net;

	internal class SagePurchaseInvoiceAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<SagePurchaseInvoice>, SagePurchaseInvoice, SageDatabaseFunctionType>
    {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<SagePurchaseInvoice>, SagePurchaseInvoice, SageDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<SagePurchaseInvoice> data, ICurrencyConvertor currencyConverter)
        {
			return new SagePurchaseInvoiceAggregator(data, currencyConverter);
        }
    }

	internal class SagePurchaseInvoiceAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<SagePurchaseInvoice>, SagePurchaseInvoice, SageDatabaseFunctionType>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SagePurchaseInvoiceAggregator));

		public SagePurchaseInvoiceAggregator(ReceivedDataListTimeDependentInfo<SagePurchaseInvoice> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
        }

		private int GetPurchaseInvoicesCount(IEnumerable<SagePurchaseInvoice> purchaseInvoices)
		{
			return purchaseInvoices.Count();
        }

		private double GetTotalSumOfPurchaseInvoices(IEnumerable<SagePurchaseInvoice> purchaseInvoices)
		{
			// consider handling currency conversion like:
			// return purchaseInvoices.Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.currency, (double)o.total_net_amount, o.date).Value);
			return purchaseInvoices.Sum(o => (double)o.total_net_amount);
		}

		protected override object InternalCalculateAggregatorValue(SageDatabaseFunctionType functionType, IEnumerable<SagePurchaseInvoice> purchaseInvoices)
        {
            switch (functionType)
            {
                case SageDatabaseFunctionType.NumOfPurchaseInvoices:
					return GetPurchaseInvoicesCount(purchaseInvoices);

				case SageDatabaseFunctionType.TotalSumOfPurchaseInvoices:
					return GetTotalSumOfPurchaseInvoices(purchaseInvoices);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}