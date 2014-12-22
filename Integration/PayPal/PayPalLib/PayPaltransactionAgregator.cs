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
			if (_period == TimePeriodEnum.Month || _period == TimePeriodEnum.Month3 || _period == TimePeriodEnum.Year)
			{
				//Serialize<List<PayPalTransactionItem>>(data.ToList(), "allData", _period.ToString());
			}
		}

		protected override object InternalCalculateAggregatorValue(PayPalDatabaseFunctionType type, IEnumerable<PayPalTransactionItem> data)
		{
			switch (type)
			{
				case PayPalDatabaseFunctionType.TotalNetInPayments: // formula 101 (not in the original Excel)
					return GetTotalNetInPayments(data);
				case PayPalDatabaseFunctionType.TotalNetInPaymentsAnnualized:
					return GetTotalNetInPaymentsAnnualized(data);
				case PayPalDatabaseFunctionType.TotalNetOutPayments: // formula 102 (not in the original Excel)
					return GetTotalNetOutPayments(data);
				case PayPalDatabaseFunctionType.TransactionsNumber: // formula 103 (not in the original Excel)
					return GetTransactionsNumber(data);
				case PayPalDatabaseFunctionType.TotalNetRevenues://formula 1
					return GetTotalNetRevenues(data);
				case PayPalDatabaseFunctionType.TotalNetExpenses://formula 2
					return GetTotalNetExpenses(data);
				case PayPalDatabaseFunctionType.NumOfTotalTransactions://formula 3
					return GetNumOfTotalTransactions(data);
				case PayPalDatabaseFunctionType.RevenuesForTransactions://formula 4
					return GetRevenuesForTransactions(data);
				case PayPalDatabaseFunctionType.NetNumOfRefundsAndReturns://formula 5
					return GetNetNumOfRefundsAndReturns(data);
				case PayPalDatabaseFunctionType.TransferAndWireOut://formula 6 sum
					return GetTransferAndWireOut(data);
				case PayPalDatabaseFunctionType.TransferAndWireIn://formula 7 sum
					return GetTransferAndWireIn(data);

				case PayPalDatabaseFunctionType.GrossIncome://Formula 1 - Formula 2
					return GetGrossIncome(data);
				case PayPalDatabaseFunctionType.GrossProfitMargin://Gross income / Formula 1
					return GetGrossProfitMargin(data);
				case PayPalDatabaseFunctionType.RevenuePerTrasnaction:// Formula 4 / Formula 3
					return GetRevenuePerTrasnaction(data);
				case PayPalDatabaseFunctionType.NetSumOfRefundsAndReturns://formula 5a
					return GetNetSumOfRefundsAndReturns(data);
				case PayPalDatabaseFunctionType.RatioNetSumOfRefundsAndReturnsToNetRevenues://Forumla 5a / Formula 1
					return GetRatioNetSumOfRefundsAndReturnsToNetRevenues(data);
				case PayPalDatabaseFunctionType.NetTransfersAmount://Formula 6 + Formula 7
					return GetNetTransfersAmount(data);
				case PayPalDatabaseFunctionType.OutstandingBalance://Gross Income - Net Transfers amount
					return GetOutstandingBalance(data);
				case PayPalDatabaseFunctionType.NumTransfersOut:// Formula 6a
					return GetNumTransfersOut(data);
				case PayPalDatabaseFunctionType.AmountPerTransferOut:// Formula 6 / Formula 6a
					return GetAmountPerTransferOut(data);
				case PayPalDatabaseFunctionType.NumTransfersIn://formula 7a
					return GetNumTransfersIn(data);
				case PayPalDatabaseFunctionType.AmountPerTransferIn://formula 7 / Formula 7a
					return GetAmountPerTransferIn(data);

				default:
					throw new NotImplementedException();
			}
		}

		private object GetAmountPerTransferIn(IEnumerable<PayPalTransactionItem> data)
		{
			var numTransfersIn = (int)GetNumTransfersIn(data);
			return numTransfersIn == 0 ? 0 : (double)GetTransferAndWireIn(data) / numTransfersIn;
		}

		private object GetNumTransfersIn(IEnumerable<PayPalTransactionItem> data)
		{
			var formula = _payPalFormulaRepository.GetByFormulaName("TransferAndWireIn").AsEnumerable();
			return GetCount(data, formula, "NumTransfersIn");
		}

		private object GetAmountPerTransferOut(IEnumerable<PayPalTransactionItem> data)
		{
			var numTransfersOut = (int)GetNumTransfersOut(data);
			return numTransfersOut == 0 ? 0 : (double)GetTransferAndWireOut(data) / numTransfersOut;
		}

		private object GetNumTransfersOut(IEnumerable<PayPalTransactionItem> data)
		{
			var formula = _payPalFormulaRepository.GetByFormulaName("TransferAndWireOut").AsEnumerable();
			return GetCount(data, formula, "NumTransfersOut");
		}

		private object GetOutstandingBalance(IEnumerable<PayPalTransactionItem> data)
		{
			return (double)GetGrossIncome(data) - Math.Abs((double)GetNetTransfersAmount(data));
		}

		private object GetNetTransfersAmount(IEnumerable<PayPalTransactionItem> data)
		{
			return (double)GetTransferAndWireOut(data) + (double)GetTransferAndWireIn(data);
		}

		private object GetRatioNetSumOfRefundsAndReturnsToNetRevenues(IEnumerable<PayPalTransactionItem> data)
		{
			var totalNetRevenues = (double)GetTotalNetRevenues(data);
			return totalNetRevenues == 0 ? 0 : (double)GetNetSumOfRefundsAndReturns(data) / totalNetRevenues;
		}

		private object GetNetSumOfRefundsAndReturns(IEnumerable<PayPalTransactionItem> data)
		{
			var formula = _payPalFormulaRepository.GetByFormulaName("NetNumOfRefundsAndReturns").AsEnumerable();
			return GetSum(data, formula, "NetSumOfRefundsAndReturns");
		}

		private object GetRevenuePerTrasnaction(IEnumerable<PayPalTransactionItem> data)
		{
			var numOfTotalTransactions = (int)GetNumOfTotalTransactions(data);
			return numOfTotalTransactions == 0 ? 0 : (double)GetRevenuesForTransactions(data) / (int)GetNumOfTotalTransactions(data);
		}

		private object GetGrossProfitMargin(IEnumerable<PayPalTransactionItem> data)
		{
			var totalNetRevenues = (double) GetTotalNetRevenues(data);
			return totalNetRevenues == 0 ? 0 : (double)GetGrossIncome(data) / totalNetRevenues;
		}

		private object GetGrossIncome(IEnumerable<PayPalTransactionItem> data)
		{
			return (double)GetTotalNetRevenues(data) - Math.Abs((double)GetTotalNetExpenses(data));
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
			return data.Count(t => t.Status == "Completed" && t.Type == "Payment" && t.NetAmount != null && t.NetAmount.Value > 0);
		}

		private object GetTotalNetOutPayments(IEnumerable<PayPalTransactionItem> data)
		{
			return data.Where(t => t.Status == "Completed" && t.Type == "Transfer" && t.NetAmount != null && t.NetAmount.Value > 0)
					   .Sum(
						   t =>
						   CurrencyConverter.ConvertToBaseCurrency(t.NetAmount.CurrencyCode, t.NetAmount.Value, t.Created).Value);
		}

		private object GetTotalNetInPayments(IEnumerable<PayPalTransactionItem> data)
		{
			return data.Where(t => t.Status == "Completed" && t.Type == "Payment" && t.NetAmount != null && t.NetAmount.Value > 0)
					   .Sum(
						   t =>
						   CurrencyConverter.ConvertToBaseCurrency(t.NetAmount.CurrencyCode, t.NetAmount.Value, t.Created).Value);
		}

		private object GetTotalNetInPaymentsAnnualized(IEnumerable<PayPalTransactionItem> data)
		{
			var dataWithExtraInfo = data as ReceivedDataListTimeDependentInfo<PayPalTransactionItem>;
			if (dataWithExtraInfo == null)
			{
				return 0;
			}
			
			double sum = (double)GetTotalNetInPayments(data);
			return AnnualizeHelper.AnnualizeSum(dataWithExtraInfo.TimePeriodType, dataWithExtraInfo.SubmittedDate, sum);
		}

		private double GetSum(IEnumerable<PayPalTransactionItem> data, IEnumerable<MP_PayPalAggregationFormula> formula, string name = "")
		{
			var result = data.Join(formula,
								   d => new { status = d.Status, type = d.Type, positive = d.NetAmount != null && d.NetAmount.Value >= 0 },
								   f => new { status = f.Status, type = f.Type, positive = f.Positive.GetValueOrDefault() }, (d, f) => d);
			//Serialize<List<PayPalTransactionItem>>(result.ToList(), name, _period.ToString());
			return result.Sum(t => t.NetAmount == null ? 0 : CurrencyConverter.ConvertToBaseCurrency(t.NetAmount.CurrencyCode, t.NetAmount.Value, t.Created).Value);
		}

		private int GetCount(IEnumerable<PayPalTransactionItem> data, IEnumerable<MP_PayPalAggregationFormula> formula, string name = "")
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
	}
}