namespace EzBob.Models.Marketplaces {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Utils;
	using EzBob.CommonLib.TimePeriodLogic;
	using EzBob.Models;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Marketplaces;

	public class PaymentAccountsModel : SimpleMarketPlaceModel {
		public PaymentAccountsModel(MP_CustomerMarketPlace mp, DateTime? history) {
			if (mp != null) {
				displayName = mp.DisplayName;
				id = mp.Id;
				IsNew = mp.IsNew;
				Status = mp.GetUpdatingStatus(history);
			}
			MonthInPayments = 0;
			TotalNetInPayments = 0;
			TotalNetOutPayments = 0;
			TransactionsNumber = 0;
		} // constructor
		public int id { get; set; }
		public bool IsNew { get; set; }

		[Traversable]
		public decimal MonthInPayments { get; set; }

		public decimal MonthInPaymentsAnnualized {
			get {
				return MonthInPayments * 12;
			}
		}

		public string Status { get; set; }

		[Traversable]
		public decimal TotalNetInPayments { get; set; }
		[Traversable]
		public decimal TotalNetOutPayments { get; set; }

		[Traversable]
		public int TransactionsNumber { get; set; }

		public virtual void Load(List<IAnalysisDataParameterInfo> av) {
			var monthIncome = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Month && x.ParameterName == AggregationFunction.Turnover.ToString());
			MonthInPayments = monthIncome == null ? 0 : (decimal)monthIncome.Value;

			var yearIncome = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.Turnover.ToString());
			TotalNetInPayments = yearIncome == null ? 0 : (decimal)yearIncome.Value;
		} // Load

	} // class PaymentAccountsModel

	public class HmrcPaymentAccountsModel : PaymentAccountsModel {
		public HmrcPaymentAccountsModel(MP_CustomerMarketPlace mp, DateTime? history)
			: base(mp, history) {
		}

		public virtual void Load(List<IAnalysisDataParameterInfo> av) {
			base.Load(av);
			var freeCashFlow = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.FreeCashFlow.ToString());
			TotalNetOutPayments = TotalNetInPayments - (freeCashFlow == null ? 0M : (decimal)freeCashFlow.Value); //netIn - netOut = freeCashflow
			TransactionsNumber = 0; //irrelevant for HMRC

		} // Load
	} // class ChannelGrabberPaymentAccountsModel

	public class FreeAgentPaymentAccountsModel : PaymentAccountsModel {
		public FreeAgentPaymentAccountsModel(MP_CustomerMarketPlace mp, DateTime? history)
			: base(mp, history) {
		}

		public virtual void Load(List<IAnalysisDataParameterInfo> av) {
			base.Load(av);
			var totalSumOfExpenses = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.TotalSumOfExpenses.ToString());
			var numOfOrders = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.NumOfOrders.ToString());
			var numOfExpenses = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.NumOfExpenses.ToString());
			TotalNetOutPayments = totalSumOfExpenses == null ? 0 : (decimal)totalSumOfExpenses.Value;
			TransactionsNumber = (numOfOrders == null ? 0 : (int)numOfOrders.Value) + (numOfExpenses == null ? 0 : (int)numOfExpenses.Value);
		} // Load
	} // class FreeAgentPaymentAccountsModel

	public class PayPalPaymentAccountsModel : PaymentAccountsModel {
		public PayPalPaymentAccountsModel(MP_CustomerMarketPlace mp, DateTime? history)
			: base(mp, history) {
		}

		public virtual void Load(List<IAnalysisDataParameterInfo> av) {
			base.Load(av);
			var totalNetOutPayments = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.TotalNetOutPayments.ToString());
			var transactionsNumber = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.TransactionsNumber.ToString());
			TotalNetOutPayments = totalNetOutPayments == null ? 0 : (decimal)totalNetOutPayments.Value;
			TransactionsNumber = transactionsNumber == null ? 0 : (int)transactionsNumber.Value;
		} // Load
	} // class PayPalPaymentAccountsModel

	public class SagePaymentAccountsModel : PaymentAccountsModel {
		public SagePaymentAccountsModel(MP_CustomerMarketPlace mp, DateTime? history)
			: base(mp, history) {
		}

		public virtual void Load(List<IAnalysisDataParameterInfo> av) {
			base.Load(av);
			var totalSumOfPurchaseInvoices = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.TotalSumOfPurchaseInvoices.ToString());
			var totalSumOfExpenditures = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.TotalSumOfExpenditures.ToString());
			var numOfIncomes = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.NumOfIncomes.ToString());
			var numOfExpenditures = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.NumOfExpenditures.ToString());
			var numOfPurchaseInvoices = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.NumOfPurchaseInvoices.ToString());
			var numOfOrders = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.NumOfOrders.ToString());

			TotalNetOutPayments = (totalSumOfPurchaseInvoices == null ? 0 : (decimal)totalSumOfPurchaseInvoices.Value) +
								  (totalSumOfExpenditures == null ? 0 : (decimal)totalSumOfExpenditures.Value);
			TransactionsNumber = (numOfIncomes == null ? 0 : (int)numOfIncomes.Value) +
								 (numOfExpenditures == null ? 0 : (int)numOfExpenditures.Value) +
								 (numOfPurchaseInvoices == null ? 0 : (int)numOfPurchaseInvoices.Value) +
								 (numOfOrders == null ? 0 : (int)numOfOrders.Value);
		} // Load
	} // class SagePaymentAccountsModel

	public class YodleePaymentAccountsModel : PaymentAccountsModel {
		public YodleePaymentAccountsModel(MP_CustomerMarketPlace mp, DateTime? history)
			: base(mp, history) {
		}

		public virtual void Load(List<IAnalysisDataParameterInfo> av) {
			base.Load(av);
			var totalExpense = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.TotalExpense.ToString());
			var numberOfTransactions = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.NumberOfTransactions.ToString());
			TotalNetOutPayments = totalExpense == null ? 0 : (decimal)totalExpense.Value;
			TransactionsNumber = numberOfTransactions == null ? 0 : (int)numberOfTransactions.Value;
		} // Load
	} // class YodleePaymentAccountsModel

		public class PayPointAccountsModel : PaymentAccountsModel {
			public PayPointAccountsModel(MP_CustomerMarketPlace mp, DateTime? history)
			: base(mp, history) {
		}

		public virtual void Load(List<IAnalysisDataParameterInfo> av) {
			base.Load(av);
			
			var numOfOrders = av.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.NumOfOrders.ToString());
			TotalNetOutPayments = 0; //irrelevant for paypoint
			TransactionsNumber = numOfOrders == null ? 0 : (int)numOfOrders.Value;
		} // Load
	} // class YodleePaymentAccountsModel
	
} // namespace
