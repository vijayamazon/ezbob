namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Marketplaces.Sage;
	using EzBob.CommonLib.TimePeriodLogic;
	using log4net;

	internal class SageSalesInvoiceAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<SageSalesInvoice>, SageSalesInvoice, SageDatabaseFunctionType>
	{
		public List<MP_SagePaymentStatus> SagePaymentStatuses { get; set; }

		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<SageSalesInvoice>, SageSalesInvoice, SageDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<SageSalesInvoice> data, ICurrencyConvertor currencyConverter)
        {
			return new SageSalesInvoiceAggregator(data, currencyConverter, SagePaymentStatuses);
        }
    }

	internal class SageSalesInvoiceAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<SageSalesInvoice>, SageSalesInvoice, SageDatabaseFunctionType>
    {
		private static readonly ILog Log = LogManager.GetLogger(typeof(SageSalesInvoiceAggregator));
		private readonly Dictionary<int, string> sagePaymentStatusesMap = new Dictionary<int, string>();

		public SageSalesInvoiceAggregator(ReceivedDataListTimeDependentInfo<SageSalesInvoice> orders, ICurrencyConvertor currencyConvertor, IEnumerable<MP_SagePaymentStatus> sagePaymentStatuses)
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

		private double GetTotalSumOfSalesInvoicesWithStatus(IEnumerable<SageSalesInvoice> purchaseInvoices, string statusName)
		{
			return purchaseInvoices.Where(o => o.status.HasValue && sagePaymentStatusesMap[o.status.Value] == statusName).Sum(o => (double)o.total_net_amount);
		}

		protected override object InternalCalculateAggregatorValue(SageDatabaseFunctionType functionType, IEnumerable<SageSalesInvoice> salesInvoices)
        {
            switch (functionType)
            {
                case SageDatabaseFunctionType.NumOfOrders:
					return GetSalesInvoicesCount(salesInvoices);

				case SageDatabaseFunctionType.TotalSumOfOrders:
					return GetTotalSumOfSalesInvoices(salesInvoices);

				case SageDatabaseFunctionType.TotalSumOfPaidSalesInvoices:
					return GetTotalSumOfSalesInvoicesWithStatus(salesInvoices, "Paid");

				case SageDatabaseFunctionType.TotalSumOfUnpaidSalesInvoices:
					return GetTotalSumOfSalesInvoicesWithStatus(salesInvoices, "Unpaid");

				case SageDatabaseFunctionType.TotalSumOfPartiallyPaidSalesInvoices:
					return GetTotalSumOfSalesInvoicesWithStatus(salesInvoices, "Part Paid");

                default:
                    throw new NotImplementedException();
            }
        }
    }
}