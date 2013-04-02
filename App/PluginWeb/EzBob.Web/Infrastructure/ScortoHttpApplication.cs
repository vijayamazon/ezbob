using System;
using System.Web;
using log4net;
using log4net.Config;
using NHibernate;
using Scorto.Configuration;
using Scorto.DBCommon;
using Scorto.NHibernate;
using Scorto.RegistryScanner;
using StructureMap;

namespace Scorto.Web.Services
{
    public class ScortoHttpApplication : System.Web.HttpApplication
    {
        private static readonly ILog _log = LogManager.GetLogger("Scorto.ScortoHttpApplication");

        private static bool _isInitialized = false;

        public static ISession CurrentSession
        {
            get
            {
                try
                {
                    if (HttpContext.Current.Items["current.session"] as ISession == null)
                    {
                        HttpContext.Current.Items["current.session"] = NHibernateManager.OpenSession();
                    }
                    return HttpContext.Current.Items["current.session"] as ISession;
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                    throw;
                }
            }
        }

        public ScortoHttpApplication()
        {
            EndRequest += (sender, args) =>
                {
                    var session = HttpContext.Current.Items["current.session"] as ISession;
                    if (session != null)
                    {
                        if (session.IsOpen)
                        {
                            CurrentSession.Close();
                        }
                        CurrentSession.Dispose();
                    }
                    HttpContext.Current.Items["current.session"] = null;
                    ThreadContext.Properties.Clear();
                };

            InitOnStart();
        }


        public virtual void InitOnStart()
        {
            base.Init();

            if (_isInitialized) return;

            lock (this)
            {
                if (_isInitialized) return;

                try
                {
                    var configuration = ConfigurationRoot.GetConfiguration();
                    XmlConfigurator.Configure(configuration.XmlElementLog);

                    DbLibConfiguration dbLibConfiguration = configuration.DbLib;
                    string libFileName = dbLibConfiguration.LibraryPath;
                    string lconnectionString = dbLibConfiguration.ConnectionString;

                    DbCommon.Init(libFileName, lconnectionString);

                    if (configuration.NHProfEnabled)
                    {
                        HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
                    }

                    Scanner.Register();
                    ConfigureStructureMap(ObjectFactory.Container);
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                    throw;
                }    
                
                _isInitialized = true;
            }
        }

        protected virtual void ConfigureStructureMap(IContainer container)
        {
            container.Configure(x =>
                                        {
                                            x.For<ISession>().Use(() => CurrentSession);
                                        });
        }
    }
}