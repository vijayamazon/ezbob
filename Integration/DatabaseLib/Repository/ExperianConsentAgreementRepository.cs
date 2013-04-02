using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using System.Linq;

namespace EZBob.DatabaseLib.Repository
{
    public interface IExperianConsentAgreementRepository : IRepository<ExperianConsentAgreement>
    {
        ExperianConsentAgreement GetByCustomerId(int customerId);
    }

    public class ExperianConsentAgreementRepository : NHibernateRepositoryBase<ExperianConsentAgreement>, IExperianConsentAgreementRepository
    {
        public ExperianConsentAgreementRepository(ISession session) : base(session)
        {
        }

        public ExperianConsentAgreement GetByCustomerId(int customerId)
        {
            return GetAll().FirstOrDefault(x => x.CustomerId == customerId);
        }
    }
}
