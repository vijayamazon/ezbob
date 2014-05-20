namespace FreeAgent
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib.TimePeriodLogic;

	internal class FreeAgentInvoiceAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<FreeAgentInvoice>, FreeAgentInvoice, FreeAgentDatabaseFunctionType>
    {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<FreeAgentInvoice>, FreeAgentInvoice, FreeAgentDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<FreeAgentInvoice> data, ICurrencyConvertor currencyConverter)
        {
			return new FreeAgentInvoiceAggregator(data, currencyConverter);
        }
    }

	internal class FreeAgentInvoiceAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<FreeAgentInvoice>, FreeAgentInvoice, FreeAgentDatabaseFunctionType>
    {
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

		private double GetTotalSumOfInvoicesAnnualized(IEnumerable<FreeAgentInvoice> invoices)
		{
			var invoicesWithExtraInfo = invoices as ReceivedDataListTimeDependentInfo<FreeAgentInvoice>;
			if (invoicesWithExtraInfo == null)
			{
				return 0;
			}

			double sum = GetTotalSumOfInvoices(invoices);
			return AnnualizeHelper.AnnualizeSum(invoicesWithExtraInfo.TimePeriodType, invoicesWithExtraInfo.SubmittedDate, sum);
		}
		
		private double GetSumOfPaidInvoices(IEnumerable<FreeAgentInvoice> invoices)
		{
			return invoices.Where(o => o.status == "Paid").Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.currency, (double)o.net_value, o.dated_on).Value);
		}

		private double GetSumOfOverdueInvoices(IEnumerable<FreeAgentInvoice> invoices)
		{
			return invoices.Where(o => o.status == "Overdue").Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.currency, (double)o.net_value, o.dated_on).Value);
		}

		private double GetSumOfOpenInvoices(IEnumerable<FreeAgentInvoice> invoices)
		{
			return invoices.Where(o => o.status == "Open").Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.currency, (double)o.net_value, o.dated_on).Value);
		}

		private double GetSumOfDraftInvoices(IEnumerable<FreeAgentInvoice> invoices)
		{
			return invoices.Where(o => o.status == "Draft").Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.currency, (double)o.net_value, o.dated_on).Value);
		}

		protected override object InternalCalculateAggregatorValue(FreeAgentDatabaseFunctionType functionType, IEnumerable<FreeAgentInvoice> invoices)
        {
            switch (functionType)
            {
                case FreeAgentDatabaseFunctionType.NumOfOrders:
					return GetInvoicesCount(invoices);

				case FreeAgentDatabaseFunctionType.TotalSumOfOrders:
					return GetTotalSumOfInvoices(invoices);

				case FreeAgentDatabaseFunctionType.SumOfPaidInvoices:
					return GetSumOfPaidInvoices(invoices);

				case FreeAgentDatabaseFunctionType.SumOfOverdueInvoices:
					return GetSumOfOverdueInvoices(invoices);

				case FreeAgentDatabaseFunctionType.SumOfOpenInvoices:
					return GetSumOfOpenInvoices(invoices);

				case FreeAgentDatabaseFunctionType.SumOfDraftInvoices:
					return GetSumOfDraftInvoices(invoices);

				case FreeAgentDatabaseFunctionType.TotalSumOfOrdersAnnualized:
					return GetTotalSumOfInvoicesAnnualized(invoices);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}