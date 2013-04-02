using EZBob.DatabaseLib.Model.Database;
using ExperianLib.Tests;
using EzBob.AmazonServiceLib.Config;
using EzBob.AmazonServiceLib.ServiceCalls;
using Moq;
using NUnit.Framework;

namespace EzBob.AmazonServiceLib.Tests
{
    internal class AmazonServiceAskvilleTests : BaseTest
    {
        [Test]
        public void TestSendForRealSeller()
        {
            var askville = new AmazonServiceAskville(GetConfigForAskville());
            var state = askville.AskQuestion("A1OXZLJTRHTZJ3", "A1F83G8C2ARO7P", 5,
                                                    "Hi! This is test message from developer, please delete it, many thanks!!!");
            Assert.IsTrue(state == AskvilleSendStatus.Success);
        }

        [Test]
        public void TestSendForUnrealSeller()
        {
            var askville = new AmazonServiceAskville(GetConfigForAskville());
            var state = askville.AskQuestion("B1OXZLJTRHTZJ3", "H1F83G8C2ARO7P", 5,
                                                    "Hi! This is test message from developer, please delete it, many thanks!!!");
            Assert.IsTrue(state == AskvilleSendStatus.InvalidSellerOrMarketplace);
        }

        private static IAmazonMarketPlaceTypeConnection GetConfigForAskville()
        {
            var config = new Mock<IAmazonMarketPlaceTypeConnection>();
            config.SetupGet(x => x.AskvilleAmazonLogin).Returns("shubin.igor90@gmail.com");
            config.SetupGet(x => x.AskvilleAmazonPass).Returns("fgkzpwms");
            return config.Object;
        }
    }
}