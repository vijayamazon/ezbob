using System.Collections.Generic;
using ApplicationMng.Repository;
using NHibernate;

namespace EzBob.Web.Models.Repository
{
    public interface IDbStringRepository : IRepository<DbString>
    {
        IList<DbString> GetAllStrings();
        string GetByKey(string key);
    }

    public class DbStringRepository : NHibernateRepositoryBase<DbString>, IDbStringRepository
    {
        public DbStringRepository(ISession session) : base(session)
        {
        }

        public IList<DbString> GetAllStrings()
        {
            return Session.QueryOver<DbString>().Cacheable().CacheRegion("DbStrings").List();
        }

        public string GetByKey(string key)
        {
            return Session.QueryOver<DbString>()
                .Where(s => s.Key == key)
                .Select(s => s.Value)
                .Cacheable()
                .CacheRegion("DbStrings")
                .SingleOrDefault<string>();
        }
    }
}