using System;
using ExperianLib.IdIdentityHub;
using NUnit.Framework;

namespace ExperianLib.Tests
{
    class EndpointTest:BaseTest
    {
        [Test]
        public void TestAndrewMilburnAMLA()
        {
            var service = new IdHubService();
            var results = service.Authenticate("Andrew", "JB", "Milburn", "M", new DateTime(1964, 12, 8), null, null, "3 Linford Close", "Rugeley", null, "WS15 4EF", 1);
            if(!results.HasError) Log.InfoFormat("AML Completed successfully");
            else Log.InfoFormat("AML Failed: " + results.Error);
        }

        [Test]
        public void TestAndrewMilburnBwa()
        {
            var service = new IdHubService();
            var results = service.AccountVerification("Andrew", "JB", "Milburn", "M", new DateTime(1964, 12, 8), null, null, "3 Linford Close", "Rugeley", null, "WS15 4EF", "110730", "00742205", 1);
            if (!results.HasError) Log.InfoFormat("BWA test passed.");
            else Log.InfoFormat("BWA test fail: " + results.Error);
        }

        [Test]
        public void TestAdamPhilpottBwa()
        {
            var service = new IdHubService();
            var results = service.AccountVerification("Adam", null, "Philpott", "M", new DateTime(1987, 12, 16), "33 North Court Close", "Rustington", null, "Littlehampton", null, "BN16 3HZ", "306782", "23356268", 1);
            if (!results.HasError) Log.InfoFormat("BWA test passed.");
            else Log.InfoFormat("BWA test fail: " + results.Error);
        }
    }
}
