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
    }
}
