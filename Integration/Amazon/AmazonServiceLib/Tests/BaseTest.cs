using System.Xml;
using NUnit.Framework;
using Scorto.Configuration;
using Scorto.Configuration.Loader;
using Scorto.RegistryScanner;
using log4net.Config;

namespace ExperianLib.Tests
{
    public class BaseTest
    {
        [SetUp]
        public void InitEnv()
        {
            EnvironmentConfigurationLoader.AppPathDummy = "test";
            XmlElement configurationElement = ConfigurationRoot.GetConfiguration().XmlElementLog;
            XmlConfigurator.Configure(configurationElement);
            Scanner.Register();
        }
    }
}
