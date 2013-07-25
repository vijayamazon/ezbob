using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.DatabaseWrapper.Transactions;
using EzBob.CommonLib.TimePeriodLogic;
using EzBob.PayPalDbLib;

namespace EzBob.PayPal
{
	using System.IO;
	using System.Xml.Serialization;
	using EZBob.DatabaseLib.Model.Marketplaces.PayPal;
	using EZBob.DatabaseLib.PayPal;
	using StructureMap;
	using log4net;

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
		private readonly TimePeriodEnum _period;
		private static readonly ILog Log = LogManager.GetLogger(typeof(PayPalTransactionAgregator));

		public PayPalTransactionAgregator(ReceivedDataListTimeDependentInfo<PayPalTransactionItem> data, ICurrencyConvertor currencyConverter)
			: base(data, currencyConverter)
		{
			_period = data.TimePeriodType;
			if (_period == TimePeriodEnum.Month || _period == TimePeriodEnum.Month3)
			{
				//Serialize<List<PayPalTransactionItem>>(data.ToList(), "allData", _period.ToString());
			}
		}

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

				case PayPalDatabaseFunctionType.TotalNetExpenses:
					return GetTotalNetExpenses(data);

				case PayPalDatabaseFunctionType.NumOfTotalTransactions:
					return GetNumOfTotalTransactions(data);

				case PayPalDatabaseFunctionType.RevenuesForTransactions:
					return GetRevenuesForTransactions(data);

				case PayPalDatabaseFunctionType.NetNumOfRefundsAndReturns:
					return GetNetNumOfRefundsAndReturns(data);

				case PayPalDatabaseFunctionType.TransferAndWireOut:
					return GetTransferAndWireOut(data);

				case PayPalDatabaseFunctionType.TransferAndWireIn:
					return GetTransferAndWireIn(data);

				default:
					throw new NotImplementedException();
			}
		}

		private object GetSum(IEnumerable<PayPalTransactionItem> data, IEnumerable<MP_PayPalAggregationFormula> formula, string name = "")
		{
			var result = data.Join(formula,
								   d => new { status = d.Status, type = d.Type, positive = d.NetAmount != null && d.NetAmount.Value >= 0 },
								   f => new { status = f.Status, type = f.Type, positive = f.Positive.GetValueOrDefault() }, (d, f) => d);
			//Serialize<List<PayPalTransactionItem>>(result.ToList(), name, _period.ToString());
			return result.Sum(t => CurrencyConverter.ConvertToBaseCurrency(t.NetAmount.CurrencyCode, t.NetAmount.Value, t.Created).Value);
		}

		private object GetCount(IEnumerable<PayPalTransactionItem> data, IEnumerable<MP_PayPalAggregationFormula> formula, string name = "")
		{
			var result = data.Join(formula,
								   d => new { status = d.Status, type = d.Type, positive = d.NetAmount != null && d.NetAmount.Value >= 0 },
								   f => new { status = f.Status, type = f.Type, positive = f.Positive.GetValueOrDefault() }, (d, f) => d);
			//Serialize<List<PayPalTransactionItem>>(result.ToList(), name, _period.ToString());
			return result.Count();
		}

		/// <summary>
		/// XML serializing for formula testing
		/// </summary>
		private void Serialize<T>(T formula, string name, string period)
		{
			try
			{
				var serializer = new XmlSerializer(typeof(T));

				using (var writer = new StreamWriter(name + period + ".xml"))
				{
					serializer.Serialize(writer, formula);
				}
			}
			catch (Exception ex)
			{
				Log.ErrorFormat("Serialize exception {0}", ex);
				
			}
			
		}

		private object GetTotalNetRevenues(IEnumerable<PayPalTransactionItem> data)
		{
			var formula = _payPalFormulaRepository.GetByFormulaName("TotalNetRevenues").AsEnumerable();
			return GetSum(data, formula, "TotalNetRevenues");
		}

		private object GetTotalNetExpenses(IEnumerable<PayPalTransactionItem> data)
		{
			var formula = _payPalFormulaRepository.GetByFormulaName("TotalNetExpenses").AsEnumerable();
			return GetSum(data, formula, "TotalNetExpenses");
		}

		private object GetNumOfTotalTransactions(IEnumerable<PayPalTransactionItem> data)
		{
			var formula = _payPalFormulaRepository.GetByFormulaName("NumOfTotalTransactions").AsEnumerable();
			return GetCount(data, formula, "NumOfTotalTransactions");
		}

		private object GetRevenuesForTransactions(IEnumerable<PayPalTransactionItem> data)
		{
			var formula = _payPalFormulaRepository.GetByFormulaName("RevenuesForTransactions").AsEnumerable();
			return GetSum(data, formula, "RevenuesForTransactions");
		}

		private object GetNetNumOfRefundsAndReturns(IEnumerable<PayPalTransactionItem> data)
		{
			var formula = _payPalFormulaRepository.GetByFormulaName("NetNumOfRefundsAndReturns").AsEnumerable();
			var result = data.Join(formula,
								   d => new { status = d.Status, type = d.Type },
								   f => new { status = f.Status, type = f.Type }, (d, f) => d);
			return result.Count();
		}

		private object GetTransferAndWireOut(IEnumerable<PayPalTransactionItem> data)
		{
			var formula = _payPalFormulaRepository.GetByFormulaName("TransferAndWireOut").AsEnumerable();
			return GetSum(data, formula, "TransferAndWireOut");
		}

		private object GetTransferAndWireIn(IEnumerable<PayPalTransactionItem> data)
		{
			var formula = _payPalFormulaRepository.GetByFormulaName("TransferAndWireIn").AsEnumerable();
			return GetSum(data, formula, "TransferAndWireIn");
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