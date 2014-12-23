namespace YodleeLib.connector {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;

	internal class YodleeTransactionsAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<YodleeTransactionItem>, YodleeTransactionItem, YodleeDatabaseFunctionType> {
		public YodleeTransactionsAggregator(
			ReceivedDataListTimeDependentInfo<YodleeTransactionItem> orders,
			ICurrencyConvertor currencyConvertor
			)
			: base(orders, currencyConvertor) { }

		protected override object InternalCalculateAggregatorValue(
			YodleeDatabaseFunctionType functionType,
			IEnumerable<YodleeTransactionItem> orders
		) {
			switch (functionType) {
			case YodleeDatabaseFunctionType.TotalIncome:
				return GetTotalIncome(orders);

			case YodleeDatabaseFunctionType.TotalExpense:
				return GetTotalExpense(orders);

			case YodleeDatabaseFunctionType.NumberOfTransactions:
				return GetNumberOfTransactions(orders);

			case YodleeDatabaseFunctionType.TotalIncomeAnnualized:
				return GetTotalIncomeAnnualized(orders);

			default:
				throw new NotImplementedException();
			}
		}

		private object GetNumberOfTransactions(IEnumerable<YodleeTransactionItem> orders) {
			return orders.Count();
		}

		private double GetTotalExpense(IEnumerable<YodleeTransactionItem> orders) {
			var totlalExpense = orders.Where(b =>
				b._Data.transactionStatusId.HasValue &&
				b._Data.transactionBaseTypeId.HasValue &&
				b._Data.transactionStatusId == (int)datatypes.TransactionStatus.Posted &&
				b._Data.transactionBaseTypeId == (int)datatypes.TransactionBaseType.Debit &&
				b._Data.transactionAmount != null &&
				b._Data.transactionAmount.amount.HasValue
				)
				.Sum(s =>
					CurrencyConverter.ConvertToBaseCurrency(
						s._Data.transactionAmount.currencyCode,
						s._Data.transactionAmount.amount.Value,
						(s._Data.postDate.date ?? s._Data.transactionDate.date)
						)
						.Value
				);

			return totlalExpense;
		}

		private double GetTotalIncome(IEnumerable<YodleeTransactionItem> orders) {
			var totalIncome = orders.Where(b =>
				b._Data.transactionStatusId.HasValue &&
				b._Data.transactionBaseTypeId.HasValue &&
				b._Data.transactionStatusId == (int)datatypes.TransactionStatus.Posted &&
				b._Data.transactionBaseTypeId == (int)datatypes.TransactionBaseType.Credit &&
				b._Data.transactionAmount != null &&
				b._Data.transactionAmount.amount.HasValue
				)
				.Sum(s =>
					CurrencyConverter.ConvertToBaseCurrency(
						s._Data.transactionAmount.currencyCode,
						s._Data.transactionAmount.amount.Value,
						(s._Data.postDate.date ?? s._Data.transactionDate.date)
						)
						.Value
				);

			return totalIncome;
		}

		private double GetTotalIncomeAnnualized(IEnumerable<YodleeTransactionItem> orders) {
			var ordersWithExtraInfo = orders as ReceivedDataListTimeDependentInfo<YodleeTransactionItem>;

			if (ordersWithExtraInfo == null)
				return 0;

			double totalSumOfOrders = GetTotalIncome(orders);

			return AnnualizeHelper.AnnualizeSum(ordersWithExtraInfo.TimePeriodType, ordersWithExtraInfo.SubmittedDate, totalSumOfOrders);
		}
	}

	internal class YodleeTransactionsAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<YodleeTransactionItem>, YodleeTransactionItem, YodleeDatabaseFunctionType> {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<YodleeTransactionItem>, YodleeTransactionItem, YodleeDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<YodleeTransactionItem> data, ICurrencyConvertor currencyConverter) {
			return new YodleeTransactionsAggregator(data, currencyConverter);
		}
	}
}
