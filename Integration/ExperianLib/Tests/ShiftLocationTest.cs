using EzBobIntegration.Web_References.Consumer;
using NUnit.Framework;

namespace ExperianLib.Tests
{
    [TestFixture]
    public class ShiftLocationTest
    {
        [Test]
        public void TestShiftLocation()
        {
            var loc = new InputLocationDetailsMultiLineLocation()
            {
                LocationLine1 = "1 Dibgate Cottages".ToUpper(),
                LocationLine2 = "Newington".ToUpper(),
                LocationLine3 = "".ToUpper(),
                LocationLine4 = "Folkestone".ToUpper(),
                LocationLine5 = "Kent".ToUpper(),
                LocationLine6 = "CT18 8BJ".ToUpper(),
            };
            loc = ConsumerService.ShiftLocation(loc);

            Assert.That(loc.LocationLine1 != null);
            Assert.That(loc.LocationLine2 != null);
            Assert.That(loc.LocationLine3 != null);
            Assert.That(loc.LocationLine4 != null);
            Assert.That(loc.LocationLine5 != null);
            Assert.That(loc.LocationLine6 == null);
        }
    }
}