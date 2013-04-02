using NUnit.Framework;
using Scorto.Configuration;
using Scorto.Configuration.Loader;
using log4net;

namespace EzBob.Tests
{
    [TestFixture]
    class Log4NetFixture
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Log4NetFixture));

        [Test]
        [Ignore]
        public void TestLog()
        {
            EnvironmentConfigurationLoader.AppPathDummy = "test";
            
            ConfigurationRoot configuration = ConfigurationRoot.GetConfiguration();
            log4net.Config.XmlConfigurator.Configure(configuration.Log4Net.InnerXml);
            Log.Error("Test error1");
            Log.Error("Test error2");
            Log.Error("Test error3");
            Log.Error("Test error4");
            Log.Error("Test error5");
            Log.Error("Test error6");
            Log.Warn("Test warn");
        }
    }

}
