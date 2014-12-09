namespace EzBob.Tests.PayPal
{
	using System;
	using System.Diagnostics;
	using EZBob.DatabaseLib.Model.Database;
	using AmazonLib;
	using EzBob.PayPal;
	using eBayLib;
	using FreeAgent;
	using Integration.ChannelGrabberFrontend;
	using NUnit.Framework;
	using PayPoint;
	using NHibernateWrapper.NHibernate;
	using StructureMap;
	using YodleeLib.connector;
	using Models.Marketplaces.Builders;
	using global::EKM;

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
            var builder = ObjectFactory.GetInstance<PayPalMarketplaceModelBuilder>();
            Measure(() =>
                {
                    var seniority = builder.GetAccountAge(mp);
                    Assert.That(seniority, Is.EqualTo("12.2"));
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
