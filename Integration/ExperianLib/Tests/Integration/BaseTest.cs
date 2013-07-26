using System.Xml;
using EZBob.DatabaseLib.Model.Database;
using log4net;
using log4net.Config;
using NHibernate;
using NUnit.Framework;
using Scorto.Configuration;
using Scorto.Configuration.Loader;
using Scorto.NHibernate;
using Scorto.RegistryScanner;
using StructureMap;
using StructureMap.Pipeline;

namespace ExperianLib.Tests.Integration
{
    [Ignore]
    [TestFixture]
    public class BaseTest
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(BaseTest));

        [SetUp]
        public void Start()
        {
            EnvironmentConfigurationLoader.AppPathDummy = "test";

            NHibernateManager.FluentAssemblies.Add(typeof(ApplicationMng.Model.Application).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);
            Scanner.Register();
            ObjectFactory.Configure(x =>
            {
                x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
                x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
            });
            XmlElement configurationElement = ConfigurationRoot.GetConfiguration().XmlElementLog;
            XmlConfigurator.Configure(configurationElement);
        }

    }
}
