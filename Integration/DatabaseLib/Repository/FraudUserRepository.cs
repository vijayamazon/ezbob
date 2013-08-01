using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Fraud;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{
    public class FraudUserRepository : NHibernateRepositoryBase<FraudUser>
    {
        public FraudUserRepository(ISession session) : base(session)
        {
        }

        public IEnumerable<FraudUser> GetByFirstLastName(string firstName, string lastName)
        {
            return GetAll().Where(x => x.FirstName == firstName && x.LastName == lastName).ToList();
        }
    }
}
