using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.HMRC
{
    using System.Globalization;
    using EzBobCommon.Currencies;

    /// <summary>
    /// Parses British pound amount from string
    /// </summary>
    public class GBPParser
    {
        public static readonly CultureInfo Culture = new CultureInfo("en-GB", false);
        
        private static readonly char[] LegalChars = {
			Convert.ToChar(65533), Convert.ToChar(163), // pound sign characters
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
			'-', ',', '.'
		};

        /// <summary>
        /// Parses the GBP.
        /// </summary>
        /// <param name="moneyString">The money string.</param>
        /// <returns></returns>
        public decimal ParseGBP(string moneyString) {
            string val = NormalizeString(moneyString);

            return decimal.Parse(val, NumberStyles.Currency, Culture);
        }

        /// <summary>
        /// Parses the GBP to money.
        /// </summary>
        /// <param name="moneyString">The money string.</param>
        /// <returns></returns>
        public Money ParseGbpToMoney(string moneyString) {
            decimal amount = ParseGBP(moneyString);

            return new Money(amount, Culture);
        }

        /// <summary>
        /// Normalizes the string.
        /// </summary>
        /// <param name="moneyString">The money string.</param>
        /// <returns></returns>
        private string NormalizeString(string moneyString)
        {

            if (string.IsNullOrWhiteSpace(moneyString))
                return "";

            moneyString = moneyString.Trim().Replace("GBP", "£").Replace(Convert.ToChar(65533), '£');

            int nPos = moneyString.Length - 1;

            while ((nPos >= 0) && LegalChars.Contains(moneyString[nPos]))
                nPos--;

            if (nPos < 0)
                return moneyString;

            return moneyString.Substring(nPos + 1);
        }
    }
}
