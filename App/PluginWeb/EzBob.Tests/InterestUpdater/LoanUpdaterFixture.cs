using System.Xml;
using EKM;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EzBob.AmazonLib;
using EzBob.PayPal;
using EzBob.eBayLib;
using FreeAgent;
using Integration.ChannelGrabberFrontend;
using NHibernate;
using NUnit.Framework;
using PayPoint;
using ScheduledServices.InterestCalculation;
using Scorto.Configuration;
using Scorto.Configuration.Loader;
using Scorto.NHibernate;
using Scorto.RegistryScanner;
using StructureMap;
using StructureMap.Pipeline;
using YodleeLib.connector;
using log4net.Config;

namespace EzBob.Tests.InterestUpdater
{
    public class LoanUpdaterFixture
    {
        private ISession _session;

        [SetUp]
        public void SetUp()
        {
            EnvironmentConfigurationLoader.AppPathDummy = @"c:\work\sss\app\maven\maven\bin\debug\maven.exe";
            NHibernateManager.FluentAssemblies.Add(typeof(ApplicationMng.Model.Application).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);

            ObjectFactory.Configure(x =>
            {
                x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
                x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
            });

	        var cfg = ConfigurationRoot.GetConfiguration();

            XmlElement configurationElement = cfg.XmlElementLog;
            XmlConfigurator.Configure(configurationElement);

            _session = ObjectFactory.GetInstance<ISession>();
        }

         [Test]
         [Ignore]
         public void update_real_loan()
         {
             var loan = _session.Get<Loan>(1045);
             var updater = new LoanUpdater();
             var tx = _session.BeginTransaction();
             
             updater.UpdateLoan(loan);
             _session.Update(loan);
             
             tx.Commit();
         }
        [Ignore]
        [Test]
        public void update_all_not_paid_off()
        {
            var iu = new ScheduledServices.InterestCalculation.InterestUpdater();
            iu.UpdateLoans();
        }
    }
}