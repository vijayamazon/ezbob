using System;
using ExperianLib.IdIdentityHub;
using NUnit.Framework;

namespace ExperianLib.Tests.Integration
{
    class EndpointTest:BaseTest
    {
        [Test]
        [Ignore("Ignore this fixture")]
        public void TestAndrewMilburnAMLA()
        {
            //real check
            var service = new IdHubService();
            var results = service.Authenticate("Andrew", "JB", "Milburn", "M", new DateTime(1964, 12, 8), null, null, "3 Linford Close", "Rugeley", null, "WS15 4EF", 1);
            Assert.That(!results.HasError);
        }

        [Test]
        [Ignore("Ignore this fixture")]
        public void TestAndrewMilburnBwa()
        {
            //real check
            var service = new IdHubService();
            var results = service.AccountVerification("Andrew", "JB", "Milburn", "M", new DateTime(1964, 12, 8), null, null, "3 Linford Close", "Rugeley", null, "WS15 4EF", "110730", "00742205", 1);
            Assert.That(!results.HasError);
        }

        [Test]
        [Ignore("Ignore this fixture")]
        public void TestAdamPhilpottBwa()
        {
            //real check
            var service = new IdHubService();
            var results = service.AccountVerification("Adam", null, "Philpott", "M", new DateTime(1987, 12, 16), "33 North Court Close", "Rustington", null, "Littlehampton", null, "BN16 3HZ", "306782", "23356268", 1);
            Assert.That(!results.HasError);
        }
    }
}
