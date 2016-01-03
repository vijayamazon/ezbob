using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.Currencies
{
    using System.Globalization;
    using Newtonsoft.Json;

    [JsonConverter(typeof(MoneyJsonSerializer))]
    public class Money {
        private readonly RegionInfo region;
        /// <summary>
        /// Initializes a new instance of the <see cref="Money"/> class.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="culture">The culture.</param>
        public Money(decimal amount, CultureInfo culture) {
            Amount = amount;
            this.region = new RegionInfo(culture.LCID);
        }

        public Money(decimal amount, RegionInfo region) {
            Amount = amount;
            this.region = region;
        }

        public Money(decimal amount, string ISOCurrencySymbol) {

            //TODO: generic conversion

            if (ISOCurrencySymbol.Equals("USD", StringComparison.InvariantCultureIgnoreCase)) {
                this.region = new RegionInfo("US");
            } else if (ISOCurrencySymbol.Equals("GBP", StringComparison.InvariantCultureIgnoreCase))
            {
                this.region = new RegionInfo("UK");
            }

            this.Amount = amount;
        }

        /// <summary>
        /// Gets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal Amount { get; private set; }

        public RegionInfo Region {
            get { return this.region; }
        }

        /// <summary>
        /// Gets the iso currency symbol.
        /// </summary>
        /// <value>
        /// The iso currency symbol.
        /// </value>
        public string ISOCurrencySymbol
        {
            get { return this.region.ISOCurrencySymbol; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Amount, ISOCurrencySymbol);
        }
    }
}
