using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Fraud;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Repository
{
    public class FraudDetectionRepository: NHibernateRepositoryBase<FraudDetection>
    {
        public FraudDetectionRepository(ISession session) : base(session)
        {
        }

        public IEnumerable<FraudDetection> GetByCustomerId(int customerId)
        {
            return GetAll().Where(x => x.CurrentCustomer.Id == customerId);
        }

        public IEnumerable<FraudDetection> GetLastDetections(int customerId)
        {
            var lastCheckDate =
                _session.Query<FraudDetection>()
                .Where(x => x.CurrentCustomer.Id == customerId)
                .Max(x => x.DateOfCheck);

            return GetByCustomerId(customerId).Where(x => x.DateOfCheck == lastCheckDate);
        }
    }
}
