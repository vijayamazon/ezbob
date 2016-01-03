namespace EzBobService.Currency
{
    using System;
    using System.Globalization;
    using EzBobCommon;
    using EzBobCommon.Currencies;
    using EzBobPersistence.Currency;
    using log4net;

    public class CurrencyConverter : ICurrencyConverter {
        private static readonly RegionInfo UkRegion = new RegionInfo(new CultureInfo("en-GB").LCID);

        [Injected]
        public ILog Log { get; set; }

        [Injected]
        public ICurrencyQueries CurrencyQueries { get; set; }

        /// <summary>
        /// Converts the currency.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="purchaseDate">The purchase date.</param>
        /// <returns></returns>
        public Money ConvertToGBP(Money amount, DateTime purchaseDate = default(DateTime)) {
            Optional<decimal> rate = CurrencyQueries.GetCurrencyHistoricalRate(amount.ISOCurrencySymbol, purchaseDate);
            if (!rate.HasValue) {
                Log.ErrorFormat("could not retrieve currency rate for '{0}' from DB", amount.ISOCurrencySymbol);
                return null;
            }

            decimal convertedRate = Math.Abs(rate.GetValue()) < 0.0000001m ? 0 : amount.Amount / rate.GetValue();

            return new Money(convertedRate, UkRegion);
        }
    }
}
