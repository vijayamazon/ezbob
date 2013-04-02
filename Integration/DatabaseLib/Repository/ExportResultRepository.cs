using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{

    public class ExportResultRepository : NHibernateRepositoryBase<ExportResult>
    {
        public ExportResultRepository(ISession session) : base(session)
        {
        }
    }
}
