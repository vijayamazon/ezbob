using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;

namespace TestApplication
{
	public static class NHibernateHelper
	{
		private static ISessionFactory _SessionFactory;

		private static ISessionFactory SessionFactory
		{
			get
			{
				if ( _SessionFactory == null )
				{
					var msSqlConfiguration = MsSqlConfiguration.MsSql2008.ConnectionString(c => c.FromConnectionStringWithKey("Main"));
					_SessionFactory = Fluently.Configure()
                        .Database(msSqlConfiguration)
						.Mappings( m => m.FluentMappings.AddFromAssemblyOf<Customer>() )
						.Mappings( m => m.FluentMappings.AddFromAssemblyOf<CustomerAddress>() )
						.ExposeConfiguration( BuildSchema )						
						.BuildSessionFactory();
				}
				return _SessionFactory;
			}
		}

		private static void BuildSchema( Configuration config )
		{
		}


		public static ISession OpenSession()
		{
			return SessionFactory.OpenSession();
		}
	}
}
