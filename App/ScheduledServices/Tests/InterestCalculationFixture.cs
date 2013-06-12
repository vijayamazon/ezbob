using System.Xml;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using NUnit.Framework;
using ScheduledServices.InterestCalculation;
using Scorto.Configuration;
using Scorto.Configuration.Loader;
using Scorto.NHibernate;
using Scorto.RegistryScanner;
using StructureMap;
using StructureMap.Pipeline;
using log4net.Config;

namespace ScheduledServices.Tests
{

    [TestFixture]
    [Ignore]
    public class InterestCalculationFixture
    {
        [SetUp]
        public void SetUp()
        {
            EnvironmentConfigurationLoader.AppPathDummy = @"c:\EzBob\App\clients\Maven\maven.exe";
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

        [Test]
        [Ignore]
        public void test()
        {
            var c = new InterestUpdater();
            c.UpdateLoans();
        }
    }
}