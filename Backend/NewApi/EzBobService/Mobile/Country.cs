using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobService.Mobile {
    using System.Collections.Concurrent;

    /// <summary>
    /// Holds country name and phone code
    /// (implicitly converts <see cref="T:EzBobService.MobilePhone.CountryName"/> to Country Object)
    /// </summary>
    public class Country {
        private static readonly ConcurrentDictionary<CountryName, Country> nameToCountries = new ConcurrentDictionary<CountryName, Country>();
        public static readonly Country UK = new Country(CountryName.UK, "+44");
        public static readonly Country IL = new Country(CountryName.IL, "+972");

        public readonly CountryName Name;
        public readonly string Code;

        private Country(CountryName name, string code)
        {
            this.Name = name;
            this.Code = code;
            nameToCountries[name] = this;
        }

        public static implicit operator Country(CountryName countryName) {
            Country country;
            nameToCountries.TryGetValue(countryName, out country);
            return country;
        }
    }
}
