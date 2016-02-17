namespace EzBob3dPartiesTests
{
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using EzBob3dParties.Amazon;
    using EzBob3dPartiesTests.Properties;
    using EzBobCommon.Configuration;
    using NUnit.Framework;
    using StructureMap;

    public class AmazonTests : TestBase
    {
        [Test]
        public async void Test() {

            string MerchantId = "A3UPCM3WB5TS4L";
            string MWSAuthToken = "amzn.mws.68a5cceb-b60a-b3f6-291c-08d4595ae879";
            string MarketplaceId = "A13V1IB3VIYZZH";

            IContainer container = InitContainer(typeof(AmazonService));
            var configManager = container.GetInstance<ConfigManager>();
            configManager.AddConfigJsonString(Encoding.UTF8.GetString(Resources.config));
            var amazonService = container.GetInstance<AmazonService>();

            amazonService.Orders.
        }
    }
}
