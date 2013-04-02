using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{
    public interface ICaisReportsHistoryRepository : IRepository<CaisReportsHistory>
    {
    }
    public class CaisReportsHistoryRepository : NHibernateRepositoryBase<CaisReportsHistory>, ICaisReportsHistoryRepository
    {
        public CaisReportsHistoryRepository(ISession session) : base(session)
        {
        }
    }
}
