namespace StandaloneInitializer
{
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Ezbob.RegistryScanner;
	using NHibernate;
	using StructureMap;
	using StructureMap.Attributes;
	using StructureMap.Pipeline;
	using NHibernateWrapper.NHibernate;

	public abstract class StandaloneApp
    {
        [SetterProperty]
        public ISession Session { get; set; }

        public abstract void Run(string[] args);

        public static void Execute<T>(string[] args) where T : StandaloneApp
        {
            try
            {
                Bootstrap.Init();
                var app = ObjectFactory.GetInstance<T>();
                app.Run(args);
            }
            finally
            {
                Console.WriteLine("Finished at {0}", DateTime.Now);
                Console.ReadLine();
            }
        }
    }

    public class Bootstrap
    {
        public static void Init()
        {
			NHibernateManager.FluentAssemblies.Add(typeof(User).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);

            Scanner.Register();

            ObjectFactory.Configure(x =>
            {
                x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.OpenSession());
                x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
            });
        }
    }
}
