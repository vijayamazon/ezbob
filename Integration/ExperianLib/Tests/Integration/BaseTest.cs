namespace ExperianLib.Tests.Integration
{
	using EZBob.DatabaseLib.Model.Database;
	using log4net;
	using NHibernate;
	using NUnit.Framework;
	using NHibernateWrapper.NHibernate;
	using Scorto.RegistryScanner;
	using StructureMap;
	using StructureMap.Pipeline;

    [Ignore]
    [TestFixture]
    public class BaseTest
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(BaseTest));

        [SetUp]
        public void Start()
        {
            NHibernateManager.FluentAssemblies.Add(typeof(ApplicationMng.Model.Application).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);
            Scanner.Register();
            ObjectFactory.Configure(x =>
            {
                x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
                x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
            });
        }
    }
}
