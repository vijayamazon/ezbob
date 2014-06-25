namespace EZBob.DatabaseLib {
	using System;
	using EZBob.DatabaseLib.Common;
	using Repository;

	public interface ICurrencyConvertor {
		AmountInfo ConvertToCurrency(AmountInfo amountInfo, DateTime? purchaseDate, string currencyTo);
		AmountInfo ConvertToCurrency(string sourceCurrency, double sourceValue, DateTime? purchaseDate, string currencyTo);
		AmountInfo ConvertToBaseCurrency(AmountInfo amountInfo, DateTime? purchaseDate);
		AmountInfo ConvertToBaseCurrency(string sourceCurrency, double sourceValue, DateTime? purchaseDate);
	} // interface ICurrencyConvertor

	public class CurrencyConvertor : ICurrencyConvertor {
		public static readonly string BaseCurrency = "GBP";

		public CurrencyConvertor(ICurrencyRateRepository currencyRateRepository) {
			m_oCurrencyRateRepository = currencyRateRepository;
		} // constructor

		public AmountInfo ConvertToBaseCurrency(string sourceCurrency, double sourceValue, DateTime? purchaseDate) {
			return ConvertToCurrency(sourceCurrency, sourceValue, purchaseDate, BaseCurrency);
		} // ConvertToBaseCurrency

		public AmountInfo ConvertToBaseCurrency(AmountInfo amountInfo, DateTime? purchaseDate) {
			return ConvertToCurrency(amountInfo, purchaseDate, BaseCurrency);
		} // ConvertToBaseCurrency

		public AmountInfo ConvertToCurrency(string sourceCurrency, double sourceValue, DateTime? purchaseDate, string currencyTo) {
			return ConvertToCurrency(new AmountInfo { CurrencyCode = sourceCurrency, Value = sourceValue }, purchaseDate, currencyTo);
		} // ConvertToCurrency

		public AmountInfo ConvertToCurrency(AmountInfo amountInfo, DateTime? purchaseDate, string currencyTo) {
			if (amountInfo == null || string.IsNullOrWhiteSpace(amountInfo.CurrencyCode) || amountInfo.CurrencyCode == currencyTo)
				return amountInfo;

			double value = m_oCurrencyRateRepository.GetCurrencyHistoricalRate(purchaseDate, amountInfo.CurrencyCode);

			return new AmountInfo {
				CurrencyCode = currencyTo,
				Value = Math.Abs(value) < 0.0000001 ? 0 : amountInfo.Value / value,
			};
		} // ConvertToCurrency

		private readonly ICurrencyRateRepository m_oCurrencyRateRepository;
	} // class CurrencyConvertor
} // namespace