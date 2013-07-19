using System;
using System.Diagnostics;
using EKM;
using EZBob.DatabaseLib.Model.Database;
using EzBob.AmazonLib;
using EzBob.Configuration;
using EzBob.Models;
using EzBob.PayPal;
using EzBob.PayPalServiceLib;
using EzBob.eBayLib;
using FreeAgent;
using Integration.ChannelGrabberFrontend;
using NUnit.Framework;
using PayPoint;
using Scorto.Configuration;
using Scorto.NHibernate;
using Scorto.RegistryScanner;
using StructureMap;
using YodleeLib.connector;

namespace EzBob.Tests.PayPal
{
    [TestFixture]
    public class SeniorityOptimizationFixture : IntegrationTestBase
    {
        protected override void Init()
        {
            base.Init();
        }

        [Test]
        [Ignore]
        public void create_model()
        {
            //get big paypal shop
            var mp = _session.Get<MP_CustomerMarketPlace>(1912);
            //just create model to see the speed
            Measure(() =>
                {
                    var model = PayPalModelBuilder.CreatePayPalAccountModelModel(mp);
                    Assert.That(model.Seniority, Is.EqualTo("12.2"));
                });

        }


        protected override void PreInit()
        {
            NHibernateManager.FluentAssemblies.Add(typeof(eBayDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(AmazonDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(PayPalDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(EkmDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(DatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(YodleeDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(PayPointDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(FreeAgentDatabaseMarketPlace).Assembly);
            //Scanner.Register();
            var localRoot = EnvironmentConfiguration.Configuration.GetCurrentConfiguration<EzBobConfigRoot>();
            ObjectFactory.Configure(x => x.For<IPayPalConfig>().Singleton().Use(localRoot.PayPalConfig));
            ObjectFactory.Configure(x => x.AddRegistry<EzBobRegistry>());
            ObjectFactory.Configure(x => x.AddRegistry<EzBob.PayPal.PluginRegistry>());

            base.PreInit();
        }

        private void Measure(Action action)
        {
            var sw = new Stopwatch();

            sw.Start();

            action();

            sw.Stop();

            Console.WriteLine("Execution took {0}ms", sw.ElapsedMilliseconds);
        }
    }
}