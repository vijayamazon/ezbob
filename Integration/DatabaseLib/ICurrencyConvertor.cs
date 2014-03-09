using System;
using EZBob.DatabaseLib.Common;

namespace EZBob.DatabaseLib
{
	using Repository;

	public interface ICurrencyConvertor
	{
		AmountInfo ConvertToCurrency(AmountInfo amountInfo, DateTime? purchaseDate, string currencyTo);
		AmountInfo ConvertToCurrency(string sourceCurrency, double sourceValue, DateTime? purchaseDate, string currencyTo);
		AmountInfo ConvertToBaseCurrency(AmountInfo amountInfo, DateTime? purchaseDate);
		AmountInfo ConvertToBaseCurrency(string sourceCurrency, double sourceValue, DateTime? purchaseDate);
	}

	public class CurrencyConvertor : ICurrencyConvertor
	{
		public static readonly string BaseCurrency = "GBP";

		private readonly ICurrencyRateRepository _CurrencyRateRepository;

		public CurrencyConvertor(ICurrencyRateRepository currencyRateRepository)
		{
			_CurrencyRateRepository = currencyRateRepository;
		}

		public AmountInfo ConvertToBaseCurrency(string sourceCurrency, double sourceValue, DateTime? purchaseDate)
		{
			return ConvertToCurrency(sourceCurrency, sourceValue, purchaseDate, BaseCurrency);
		}

		public AmountInfo ConvertToBaseCurrency(AmountInfo amountInfo, DateTime? purchaseDate)
		{
			return ConvertToCurrency(amountInfo, purchaseDate, BaseCurrency);
		}

		public AmountInfo ConvertToCurrency(string sourceCurrency, double sourceValue, DateTime? purchaseDate, string currencyTo)
		{
			return ConvertToCurrency(new AmountInfo { CurrencyCode = sourceCurrency, Value = sourceValue }, purchaseDate, currencyTo);
		}

		public AmountInfo ConvertToCurrency(AmountInfo amountInfo, DateTime? purchaseDate, string currencyTo)
		{
			if (amountInfo == null || string.IsNullOrWhiteSpace(amountInfo.CurrencyCode) || amountInfo.CurrencyCode == currencyTo)
			{
				return amountInfo;
			}
			else
			{
				double value = _CurrencyRateRepository.GetCurrencyHistoricalRate(purchaseDate, amountInfo.CurrencyCode);

				return new AmountInfo
					{
						CurrencyCode = currencyTo,
						Value = value == 0 ? 0 : amountInfo.Value / value
					};
			}
		}
	}
}