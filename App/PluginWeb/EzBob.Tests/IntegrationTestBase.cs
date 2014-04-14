using System.Xml;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using NUnit.Framework;
using NHibernateWrapper.NHibernate;
using StructureMap;
using StructureMap.Pipeline;
using log4net.Config;

namespace EzBob.Tests
{
    [TestFixture]
    public class IntegrationTestBase
    {
        protected ISession _session;

        [SetUp]
        public void SetUp()
        {
            NHibernateManager.FluentAssemblies.Add(typeof(ApplicationMng.Model.Application).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);

            PreInit();

            ObjectFactory.Configure(x =>
            {
                x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
                x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
            });
			
            _session = ObjectFactory.GetInstance<ISession>();

            Init();
        }

        protected virtual void PreInit()
        {
        }

        protected virtual void Init(){}
    }
}