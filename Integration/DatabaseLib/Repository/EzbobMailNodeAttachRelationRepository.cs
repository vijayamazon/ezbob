using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
    public class EzbobMailNodeAttachRelationRepository : NHibernateRepositoryBase<EzbobMailNodeAttachRelation>
    {
        public EzbobMailNodeAttachRelationRepository(ISession session) : base(session)
        {
        }
    }
}
