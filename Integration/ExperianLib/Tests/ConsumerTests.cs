using System;
using NUnit.Framework;
using EzBobIntegration.Web_References.Consumer;

namespace ExperianLib.Tests
{
    class ConsumerTests : BaseTest
    {
        [Test]
        public void TestConsumer()
        {
            var service = new ConsumerService();
            var loc = new InputLocationDetailsMultiLineLocation
            {
                LocationLine1 = "1 Dibgate Cottages".ToUpper(),
                LocationLine2 = "Newington".ToUpper(),
                LocationLine3 = "".ToUpper(),
                LocationLine4 = "Folkestone".ToUpper(),
                LocationLine5 = "Kent".ToUpper(),
                LocationLine6 = "CT18 8BJ".ToUpper(),
            };
            var dob = new DateTime(1990, 08, 17);
            var result = service.GetConsumerInfo("ABUL", "TestSurnameDebugMode", "1170", dob, null, loc, "PL", 39, 1);
            if(result.IsError)
            {
                Log.Error("Error from consumer service: " + result.Error);
                Assert.Fail();
            }
        }

        [Test]
        [Ignore]
        public void TestRealConsumer()
        {
            //Work only with experian real certificates
            var service = new ConsumerService();
            var loc = new InputLocationDetailsMultiLineLocation()
            {
                LocationLine1 = "196 High Road".ToUpper(),
                LocationLine2 = "London".ToUpper(),
                LocationLine3 = "N22 8HH".ToUpper(),
                LocationLine4 = "".ToUpper(),
                LocationLine5 = "".ToUpper(),
                LocationLine6 = "".ToUpper(),
            };
            var dob = new DateTime(1978, 02, 09);
            var result = service.GetConsumerInfo("Roy", "Loewenberg", "M", dob, null, loc, "PL", 1, 0);
            if (result.IsError)
            {
                Log.Error("Error from consumer service: " + result.Error);
                Assert.Fail();
            }
        }

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
            loc = ConsumerService.ShifLocation(loc);

            Assert.That(loc.LocationLine1 != null);
            Assert.That(loc.LocationLine2 != null);
            Assert.That(loc.LocationLine3 != null);
            Assert.That(loc.LocationLine4 != null);
            Assert.That(loc.LocationLine5 != null);
            Assert.That(loc.LocationLine6 == null);
        }
    }
}
