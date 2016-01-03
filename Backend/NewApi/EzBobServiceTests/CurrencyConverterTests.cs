namespace EzBobServiceTests
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using EzBobCommon.Currencies;
    using EzBobPersistence;
    using EzBobService.Currency;
    using NUnit.Framework;

    [TestFixture]
    public class CurrencyConverterTests : TestBase
    {
        [Test]
        public void Test() {
            var container = InitContainer(typeof(ICurrencyConverter));

            var currencyConverter = container.GetInstance<ICurrencyConverter>();

            var tenDollars = new Money(10, CultureInfo.GetCultureInfo("en-US"));

            var tenDollarsInPounds = currencyConverter.ConvertToGBP(tenDollars);

            //valid until dollar will remain cheaper then pound 
            Assert.True(tenDollars.Amount > tenDollarsInPounds.Amount);
        }
    }
}
