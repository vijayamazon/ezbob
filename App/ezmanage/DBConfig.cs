using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;

namespace ezmanage
{
    public class DBConfig
    {
        private static ISessionFactory _factory;

        static DBConfig ()
        {
            _factory = CreateSessionFactory();
        }

        public static ISession OpenSession()
        {
            return _factory.OpenSession();
        }

        public static ISessionFactory GetFactory ()
        {
            return _factory;
        }

        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
				.Database(MsSqlConfiguration.MsSql2005.ConnectionString(c => c.FromAppSetting("DBConnection"))) //DBConnectionLocal
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Customer>())
                .ExposeConfiguration(BuildSchema)
                .BuildSessionFactory();
        }
        private static void BuildSchema(Configuration config)
        {
        }

        public static IStatelessSession OpenStatelessSession()
        {
            return _factory.OpenStatelessSession();
        }
    }
}