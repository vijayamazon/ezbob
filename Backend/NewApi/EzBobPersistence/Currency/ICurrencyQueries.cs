namespace EzBobPersistence.Currency
{
    using System;
    using EzBobCommon;
    using EzBobCommon.Currencies;

    public interface ICurrencyQueries {
        /// <summary>
        /// Gets the currency historical rate.
        /// </summary>
        /// <param name="currencyISOName">ISO Name of the currency.</param>
        /// <param name="purchaseDataUtc">The purchase data UTC.</param>
        /// <returns></returns>
        Optional<decimal> GetCurrencyHistoricalRate(string currencyISOName, DateTime purchaseDataUtc = default(DateTime));
        /// <summary>
        /// Gets the currency identifier.
        /// </summary>
        /// <param name="currencyISOName">ISO Name of the currency.</param>
        /// <returns></returns>
        int GetCurrencyId(string currencyISOName);

        /// <summary>
        /// Saves the currency rate.
        /// </summary>
        /// <param name="money">The money.</param>
        /// <param name="lastUpdatedUtc">The last updated UTC.</param>
        /// <returns></returns>
        bool SaveCurrencyRate(Money money, DateTime lastUpdatedUtc);
    }
}
