using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Fraud;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{
    public class FraudDetectionRepository: NHibernateRepositoryBase<FraudDetection>
    {
        public FraudDetectionRepository(ISession session) : base(session)
        {
        }
    }
}
