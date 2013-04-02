using System;

namespace CustomSchedulers.Providers
{
	internal interface ICurrencyRateProvider
	{
		CurrencyRateProviderType ServiceType { get; }
		CurrencyRateHistoryContainer RetriveData( string currencyName, DateTime startDate, DateTime endDate );
	}

	internal enum CurrencyRateProviderType
	{
		Yahoo,
		MSN
	}
}