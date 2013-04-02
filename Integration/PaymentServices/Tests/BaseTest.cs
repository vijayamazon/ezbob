using NUnit.Framework;
using Scorto.Configuration.Loader;

namespace PaymentServices.Tests
{
    public class BaseTest
    {
        [SetUp]
        public void InitEnv()
        {
            EnvironmentConfigurationLoader.AppPathDummy = "test";
        }
    }
}
