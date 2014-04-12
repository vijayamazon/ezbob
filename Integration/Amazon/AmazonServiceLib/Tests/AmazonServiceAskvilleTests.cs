namespace EzBob.AmazonServiceLib.Tests
{
	using EZBob.DatabaseLib.Model.Database;
	using ServiceCalls;
	using NUnit.Framework;
	using Ezbob.RegistryScanner;

	internal class AmazonServiceAskvilleTests
	{
		[SetUp]
		public void InitEnv()
		{
			Scanner.Register();
		}

        [Test]
        public void TestSendForRealSeller()
        {
			var askville = new AmazonServiceAskville("shubin.igor90@gmail.com", "fgkzpwms");
            var state = askville.AskQuestion("A1OXZLJTRHTZJ3", "A1F83G8C2ARO7P", 5,
                                                    "Hi! This is test message from developer, please delete it, many thanks!!!");
            Assert.IsTrue(state == AskvilleSendStatus.Success);
        }

        [Test]
        public void TestSendForUnrealSeller()
        {
			var askville = new AmazonServiceAskville("shubin.igor90@gmail.com", "fgkzpwms");
            var state = askville.AskQuestion("B1OXZLJTRHTZJ3", "H1F83G8C2ARO7P", 5,
                                                    "Hi! This is test message from developer, please delete it, many thanks!!!");
            Assert.IsTrue(state == AskvilleSendStatus.InvalidSellerOrMarketplace);
        }
    }
}