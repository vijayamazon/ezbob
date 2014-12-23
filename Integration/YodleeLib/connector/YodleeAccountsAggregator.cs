namespace YodleeLib.connector {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;

	internal class YodleeAccountsAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<YodleeAccountItem>, YodleeAccountItem, YodleeDatabaseFunctionType> {
		public YodleeAccountsAggregator(ReceivedDataListTimeDependentInfo<YodleeAccountItem> orders, ICurrencyConvertor currencyConvertor)
			: base(orders, currencyConvertor) { }

		protected override object InternalCalculateAggregatorValue(YodleeDatabaseFunctionType functionType, IEnumerable<YodleeAccountItem> orders) {
			switch (functionType) {
			case YodleeDatabaseFunctionType.CurrentBalance:
				return GetCurrentBalance(orders);

			case YodleeDatabaseFunctionType.AvailableBalance:
				return GetAvailableBalance(orders);

			default:
				throw new NotImplementedException();
			}
		}

		private double GetAvailableBalance(IEnumerable<YodleeAccountItem> orders) {
			var availableBalance = orders
				.Where(x => x._Data.availableBalance != null && x._Data.availableBalance.amount.HasValue)
				.Sum(x =>
					CurrencyConverter.ConvertToBaseCurrency(
						x._Data.availableBalance.currencyCode,
						x._Data.availableBalance.amount.Value,
						(x._Data.asOfDate != null && x._Data.asOfDate.date.HasValue) ? x._Data.asOfDate.date : null
					)
					.Value
				);
			return availableBalance;
		}

		private double GetCurrentBalance(IEnumerable<YodleeAccountItem> orders) {
			var currentBalance = orders
				.Where(x => x._Data.currentBalance != null && x._Data.currentBalance.amount.HasValue)
				.Sum(x =>
					CurrencyConverter.ConvertToBaseCurrency(
						x._Data.currentBalance.currencyCode,
						x._Data.currentBalance.amount.Value,
						(x._Data.asOfDate != null && x._Data.asOfDate.date.HasValue) ? x._Data.asOfDate.date : null
					)
					.Value
				);
			return currentBalance;
		}
	}

	internal class YodleeAccountsAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<YodleeAccountItem>, YodleeAccountItem, YodleeDatabaseFunctionType> {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<YodleeAccountItem>, YodleeAccountItem, YodleeDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<YodleeAccountItem> data, ICurrencyConvertor currencyConverter) {
			return new YodleeAccountsAggregator(data, currencyConverter);
		}
	}
}
