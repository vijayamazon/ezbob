using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using Scorto.Configuration;
using Scorto.Configuration.Loader;
using Scorto.NHibernate;
using Scorto.RegistryScanner;
using StructureMap;
using StructureMap.Attributes;
using StructureMap.Pipeline;
using log4net.Config;

namespace StandaloneInitializer
{

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
            EnvironmentConfigurationLoader.AppPathDummy = @"c:\work\ezbob\app\pluginweb\EzBob.Web\";
            NHibernateManager.FluentAssemblies.Add(typeof(ApplicationMng.Model.Application).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);

            Scanner.Register();

            //ObjectFactory.Configure(x => x.AddRegistry<EzBobRegistry>());

            ObjectFactory.Configure(x =>
            {
                x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
                x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
            });

            var cfg = ConfigurationRoot.GetConfiguration();

            XmlElement configurationElement = cfg.XmlElementLog;
            XmlConfigurator.Configure(configurationElement);
        }
    }
}
