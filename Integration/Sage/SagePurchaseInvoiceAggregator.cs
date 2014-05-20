namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Marketplaces.Sage;
	using EzBob.CommonLib.TimePeriodLogic;

	internal class SagePurchaseInvoiceAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<SagePurchaseInvoice>, SagePurchaseInvoice, SageDatabaseFunctionType>
	{
		public List<MP_SagePaymentStatus> SagePaymentStatuses { get; set; }

		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<SagePurchaseInvoice>, SagePurchaseInvoice, SageDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<SagePurchaseInvoice> data, ICurrencyConvertor currencyConverter)
        {
			return new SagePurchaseInvoiceAggregator(data, currencyConverter, SagePaymentStatuses);
        }
    }

	internal class SagePurchaseInvoiceAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<SagePurchaseInvoice>, SagePurchaseInvoice, SageDatabaseFunctionType>
    {
		private readonly Dictionary<int, string> sagePaymentStatusesMap = new Dictionary<int, string>();

		public SagePurchaseInvoiceAggregator(ReceivedDataListTimeDependentInfo<SagePurchaseInvoice> orders, ICurrencyConvertor currencyConvertor, IEnumerable<MP_SagePaymentStatus> sagePaymentStatuses)
            : base(orders, currencyConvertor)
		{
			foreach (var paymentStatus in sagePaymentStatuses)
			{
				if (!sagePaymentStatusesMap.ContainsKey(paymentStatus.SageId))
				{
					sagePaymentStatusesMap.Add(paymentStatus.SageId, paymentStatus.name);
				}
			}
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

		private double GetTotalSumOfPurchaseInvoicesWithStatus(IEnumerable<SagePurchaseInvoice> purchaseInvoices, string statusName)
		{
			return purchaseInvoices.Where(o => o.status.HasValue && sagePaymentStatusesMap[o.status.Value] == statusName).Sum(o => (double)o.total_net_amount);
		}

		protected override object InternalCalculateAggregatorValue(SageDatabaseFunctionType functionType, IEnumerable<SagePurchaseInvoice> purchaseInvoices)
        {
            switch (functionType)
            {
                case SageDatabaseFunctionType.NumOfPurchaseInvoices:
					return GetPurchaseInvoicesCount(purchaseInvoices);

				case SageDatabaseFunctionType.TotalSumOfPurchaseInvoices:
					return GetTotalSumOfPurchaseInvoices(purchaseInvoices);

				case SageDatabaseFunctionType.TotalSumOfPaidPurchaseInvoices:
					return GetTotalSumOfPurchaseInvoicesWithStatus(purchaseInvoices, "Paid");

				case SageDatabaseFunctionType.TotalSumOfUnpaidPurchaseInvoices:
					return GetTotalSumOfPurchaseInvoicesWithStatus(purchaseInvoices, "Unpaid");

				case SageDatabaseFunctionType.TotalSumOfPartiallyPaidPurchaseInvoices:
					return GetTotalSumOfPurchaseInvoicesWithStatus(purchaseInvoices, "Part Paid");

                default:
                    throw new NotImplementedException();
            }
        }
    }
}