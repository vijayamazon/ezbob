namespace EzBob.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using FluentNHibernate.Cfg;
	using FluentNHibernate.Cfg.Db;
	using FluentNHibernate.Conventions.Helpers;
	using NHibernate;
	using NHibernate.Cfg;
	using NHibernate.Tool.hbm2ddl;
	using System.Linq;

	public abstract class InMemoryDbTestFixtureBase
	{
		protected static ISessionFactory SessionFactory;
		private static Configuration configuration;
		public List<Assembly> HbmAssemblies { get; set; }

		protected InMemoryDbTestFixtureBase()
		{
			HbmAssemblies = new List<Assembly>();
		}

		protected virtual string ConnectionString
		{
			get
			{
				return null;
			}
		}

		public void InitialiseNHibernate(params Type[] types)
		{
			InitialiseNHibernate((from t in types select t.Assembly).ToArray<Assembly>());
		}

		public void InitialiseNHibernate(params Assembly[] assemblies)
		{
			SessionFactory = Fluently.Configure().Database((SQLiteConfiguration.Standard.InMemory().FormatSql().ShowSql)).Mappings(delegate(MappingConfiguration m)
			{
				m.FluentMappings.Conventions.Add(AutoImport.Never());
				foreach (Assembly assembly in HbmAssemblies)
				{
					m.HbmMappings.AddFromAssembly(assembly);
				}
				Assembly[] assemblies2 = assemblies;
				foreach (Assembly assembly in assemblies2)
				{
					m.FluentMappings.AddFromAssembly(assembly);
				}
			}).ExposeConfiguration((BuildSchema)).BuildSessionFactory();
		}

		private void BuildSchema(Configuration localConfiguration)
		{
			configuration = localConfiguration;
		}

		public virtual ISession CreateSession()
		{
			ISession session = SessionFactory.OpenSession();
			new SchemaExport(configuration).Execute(true, true, false, session.Connection, null);
			return session;
		}
		public static ISession CreateSessionStatic()
		{
			ISession session = SessionFactory.OpenSession();
			new SchemaExport(configuration).Execute(true, true, false, session.Connection, null);
			return session;
		}
	}
}
