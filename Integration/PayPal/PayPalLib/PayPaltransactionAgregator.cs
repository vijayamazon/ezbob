using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Transactions;
using EzBob.CommonLib.TimePeriodLogic;
using EzBob.PayPalDbLib;

namespace EzBob.PayPal
{
	using EZBob.DatabaseLib.PyaPalDetails;
	using StructureMap;

	internal class PayPalTransactionAgregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<PayPalTransactionItem>, PayPalTransactionItem, PayPalDatabaseFunctionType>
	{
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<PayPalTransactionItem>, PayPalTransactionItem, PayPalDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<PayPalTransactionItem> data, ICurrencyConvertor currencyConverter)
		{
			return new PayPalTransactionAgregator(data, currencyConverter);
		}
	}

	internal class PayPalTransactionAgregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<PayPalTransactionItem>, PayPalTransactionItem, PayPalDatabaseFunctionType>
	{
		private readonly IPayPalAggregationFormulaRepository _payPalFormulaRepository = ObjectFactory.GetInstance<IPayPalAggregationFormulaRepository>();
		public PayPalTransactionAgregator(ReceivedDataListTimeDependentInfo<PayPalTransactionItem> data, ICurrencyConvertor currencyConverter)
			: base(data, currencyConverter) { }

		protected override object InternalCalculateAggregatorValue(PayPalDatabaseFunctionType type, IEnumerable<PayPalTransactionItem> data)
		{
			switch (type)
			{
				case PayPalDatabaseFunctionType.TotalNetInPayments:
					return GetTotalNetInPayments(data);

				case PayPalDatabaseFunctionType.TotalNetOutPayments:
					return GetTotalNetOutPayments(data);

				case PayPalDatabaseFunctionType.TransactionsNumber:
					return GetTransactionsNumber(data);

				case PayPalDatabaseFunctionType.TotalNetRevenues:
					return GetTotalNetRevenues(data);

				default:
					throw new NotImplementedException();
			}
		}

		private object GetTotalNetRevenues(IEnumerable<PayPalTransactionItem> data)
		{
			var formula = _payPalFormulaRepository.GetByFormulaName("TotalNetRevenues").AsEnumerable();
			var result = data.Join(formula,
								   d => new { status = d.Status, type = d.Type, positive = d.NetAmount.Value >= 0 },
								   f => new { status = f.Status, type = f.Type, positive = f.Positive }, (d, f) => d);
			return result.Sum(t => CurrencyConverter.ConvertToBaseCurrency(t.NetAmount.CurrencyCode, t.NetAmount.Value, t.Created).Value);

			/*
			return data
				.Where(t =>
						(t.Status == "Completed" && t.Type == "Payment" && t.NetAmount.Value > 0) ||
						(t.Status == "Cleared" && t.Type == "Payment" && t.NetAmount.Value > 0) ||
						(t.Status == "Pending" && t.Type == "Payment" && t.NetAmount.Value < 0) ||
						(t.Status == "Returned" && t.Type == "Payment" && t.NetAmount.Value < 0) ||
						(t.Status == "completed&refunded" && t.Type == "Recurring Payment" && t.NetAmount.Value > 0) ||
						(t.Status == "Refunded by eBay" && t.Type == "Payment" && t.NetAmount.Value > 0) ||
						(t.Status == "Partially Refunded" && t.Type == "Payment" && t.NetAmount.Value > 0) ||
						(t.Status == "Completed" && t.Type == "Refund" && t.NetAmount.Value < 0) ||
						(t.Status == "Completed" && t.Type == "Cash Back Bonus" && t.NetAmount.Value > 0) ||
						(t.Status == "Completed" && t.Type == "Bonus" && t.NetAmount.Value > 0) ||
						(t.Status == "Completed" && t.Type == "Fee" && t.NetAmount.Value < 0) ||
						(t.Status == "released" && t.Type == "Reserve Release" && t.NetAmount.Value > 0) ||
						(t.Status == "placed" && t.Type == "Reserve Hold" && t.NetAmount.Value < 0))
				.Sum(t => CurrencyConverter.ConvertToBaseCurrency(t.NetAmount.CurrencyCode, t.NetAmount.Value, t.Created).Value);
			*/
		}

		private object GetTransactionsNumber(IEnumerable<PayPalTransactionItem> data)
		{
			return data.Count(t => t.Status == "Completed" && t.Type == "Payment" && t.NetAmount.Value > 0);
		}

		private object GetTotalNetOutPayments(IEnumerable<PayPalTransactionItem> data)
		{
			return data.Where(t => t.Status == "Completed" && t.Type == "Transfer" && t.NetAmount.Value > 0)
					   .Sum(
						   t =>
						   CurrencyConverter.ConvertToBaseCurrency(t.NetAmount.CurrencyCode, t.NetAmount.Value, t.Created).Value);
		}

		private object GetTotalNetInPayments(IEnumerable<PayPalTransactionItem> data)
		{
			return data.Where(t => t.Status == "Completed" && t.Type == "Payment" && t.NetAmount.Value > 0)
					   .Sum(
						   t =>
						   CurrencyConverter.ConvertToBaseCurrency(t.NetAmount.CurrencyCode, t.NetAmount.Value, t.Created).Value);
		}
	}
}