namespace EzBobService.Currency
{
    using System;
    using EzBobCommon.Currencies;

    public interface ICurrencyConverter {

        /// <summary>
        /// Converts the currency.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="purchaseDate">The purchase date.</param>
        /// <returns></returns>
        Money ConvertToGBP(Money amount, DateTime purchaseDate = default(DateTime));
    }
}
