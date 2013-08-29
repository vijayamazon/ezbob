using System;
using EzBobIntegration.Web_References.Consumer;
using NUnit.Framework;

namespace ExperianLib.Tests.Integration
{
    class ConsumerTests : BaseTest
    {
        [Test]
        [Ignore]
        public void TestConsumer()
        {
            //debug mode
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
                LocationLine1 = "Flat".ToUpper(),
                LocationLine2 = "Gardeners Cottage".ToUpper(),
                LocationLine3 = "Lugwardine".ToUpper(),
                LocationLine4 = "Hereford".ToUpper(),
                LocationLine5 = "Herefordshire".ToUpper(),
                LocationLine6 = "HR1 4DF".ToUpper(),
            };
            var dob = new DateTime(1968, 10, 15);
            var result = service.GetConsumerInfo("Matt", "Lunt", "M", dob, null, loc, "PL", 1, 0);
            if (result.IsError)
            {
                Log.Error("Error from consumer service: " + result.Error);
                Assert.Fail();
            }
        }
    }
}
