using System;

namespace CustomSchedulers.Providers
{
	internal static class CurrencyRateProviderFactory
	{
		public static ICurrencyRateProvider Create( CurrencyRateProviderType type )
		{
			switch (type)
			{
				case CurrencyRateProviderType.Yahoo:
					return new CurrencyRateProviderYahoo();
					
				case CurrencyRateProviderType.MSN:
					return new CurrencyRateProviderMsn();

				default:
					throw new NotImplementedException();
			}
		}
	}
}