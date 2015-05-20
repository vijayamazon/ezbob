namespace NHibernateWrapper.NHibernate
{
	using FluentNHibernate.Cfg;
	using FluentNHibernate.Conventions;
	using FluentNHibernate.Conventions.Helpers;
	using global::NHibernate;
	using log4net;
	using System.IO;
	using System.Linq;
	using System.Xml;
	using System.Configuration;

	public class NHibernateManager
	{
		private static readonly object _lock = new object();
		private static ILog _log = LogManager.GetLogger("NHibernateWrapper.NHibernate.NHibernateManager");
		private static ISessionFactory _sessionFactory;
		private static global::NHibernate.Cfg.Configuration _config;
		private static readonly System.Collections.Generic.List<System.Reflection.Assembly> _fluentAssemblies = new System.Collections.Generic.List<System.Reflection.Assembly>();
		private static readonly System.Collections.Generic.List<System.Reflection.Assembly> _hbmAssemblies = new System.Collections.Generic.List<System.Reflection.Assembly>();
		public static global::NHibernate.Cfg.Configuration Configuration
		{
			get
			{
				if (NHibernateManager._config == null)
				{
					try
					{
						NHibernateManager._config = new global::NHibernate.Cfg.Configuration();
						NHibernateManager.Configure();
					}
					catch
					{
						NHibernateManager._config = null;
						throw;
					}
				}
				return NHibernateManager._config;
			}
		}
		public static ISessionFactory SessionFactory
		{
			get
			{
				ISessionFactory sessionFactory;
				lock (NHibernateManager._lock)
				{
					if (NHibernateManager._sessionFactory == null)
					{
						try
						{
							global::NHibernate.Cfg.Configuration configuration = NHibernateManager.Configuration;
							FluentConfiguration fluentConfiguration = Fluently.Configure(configuration).Mappings(new System.Action<MappingConfiguration>(NHibernateManager.AddMappings));
							NHibernateManager._sessionFactory = fluentConfiguration.BuildSessionFactory();
						}
						catch (System.Exception message)
						{
							NHibernateManager._log.Fatal("Cannot init NHibernate");
							NHibernateManager._log.Error(message);
							throw;
						}
					}
					sessionFactory = NHibernateManager._sessionFactory;
				}
				return sessionFactory;
			}
		}
		public static System.Collections.Generic.List<System.Reflection.Assembly> FluentAssemblies
		{
			get
			{
				return NHibernateManager._fluentAssemblies;
			}
		}
		public static System.Collections.Generic.List<System.Reflection.Assembly> HbmAssemblies
		{
			get
			{
				return NHibernateManager._hbmAssemblies;
			}
		}
		private static void Configure()
		{
            
			_config.Configure(XmlReader.Create(new StringReader(
                "<hibernate-configuration xmlns=\"urn:nhibernate-configuration-2.2\">" +
			        "<session-factory>" +
			            "<property name=\"cache.provider_class\">NHibernate.Caches.SysCache2.SysCacheProvider, NHibernate.Caches.SysCache2</property>" +
			            "<property name=\"cache.use_query_cache\">true</property>" +
			            "<property name=\"connection.provider\">NHibernate.Connection.DriverConnectionProvider</property>" +
			            "<property name=\"dialect\">NHibernate.Dialect.MsSql2008Dialect</property>" +
			            "<property name=\"connection.driver_class\">NHibernate.Driver.SqlClientDriver</property>" +
			            "<property name=\"show_sql\">false</property>" +
			            "<property name=\"cache.use_second_level_cache\">true</property>" +
			            "<property name=\"adonet.batch_size\">50</property>" +
			        "</session-factory>" +
                "</hibernate-configuration>")));
		
			string connectionString = ConfigurationManager.ConnectionStrings[new Ezbob.Context.Environment().Context.ToLower()].ConnectionString;
			_config.Properties.Add("connection.connection_string", connectionString);
		}

		private static void AddMappings(MappingConfiguration mappings)
		{
			mappings.FluentMappings.Conventions.Add<IHibernateMappingConvention>(AutoImport.Never());
			foreach (System.Reflection.Assembly current in NHibernateManager.FluentAssemblies.Distinct<System.Reflection.Assembly>().ToList<System.Reflection.Assembly>())
			{
				mappings.FluentMappings.AddFromAssembly(current);
			}
			foreach (System.Reflection.Assembly current in NHibernateManager.HbmAssemblies.Distinct<System.Reflection.Assembly>().ToList<System.Reflection.Assembly>())
			{
				mappings.HbmMappings.AddFromAssembly(current);
			}
		}
		public static ISession OpenSession()
		{
			ISession session = NHibernateManager.SessionFactory.OpenSession();
			session.FlushMode = FlushMode.Commit;
			return session;
		}
	}
}
