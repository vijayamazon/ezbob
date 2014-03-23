using FluentNHibernate.Cfg;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Helpers;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using Scorto.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
namespace NHibernateWrapper.NHibernate
{
	public class NHibernateManager
	{
		private static readonly object _lock = new object();
		private static ILog _log = LogManager.GetLogger("NHibernateWrapper.NHibernate.NHibernateManager");
		private static ISessionFactory _sessionFactory;
		private static global::NHibernate.Cfg.Configuration _config;
		private static bool _isOracle;
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
					catch (System.Exception var_0_25)
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
			ConfigurationRoot configuration = ConfigurationRoot.GetConfiguration();
			System.Xml.XmlElement innerXml = configuration.NHibernateConfig.InnerXml;
			NHibernateManager._config.Configure(System.Xml.XmlReader.Create(new System.IO.StringReader(innerXml.OuterXml)));
			NHibernateManager._config.Properties.Add("connection.connection_string", configuration.DbLib.ConnectionString);
			NHibernateManager._isOracle = NHibernateManager._config.GetProperty("dialect").ToLower().Contains("oracle");
		}
		private static void BuildSchema(global::NHibernate.Cfg.Configuration config)
		{
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
			if (NHibernateManager._isOracle)
			{
				using (System.Data.IDbCommand dbCommand = session.Connection.CreateCommand())
				{
					dbCommand.CommandText = "alter session set nls_date_format='YYYY-MM-DD'";
					dbCommand.ExecuteNonQuery();
				}
			}
			return session;
		}
	}
}
