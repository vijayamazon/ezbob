using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{
    public class AskvilleRepository : NHibernateRepositoryBase<Askville>
    {
        public AskvilleRepository(ISession session) : base(session)
        {
        }

        public Askville GetAskvilleByGuid(string guid)
        {
            return GetAll().FirstOrDefault(x => x.Guid == guid);
        }

        public Askville GetAskvilleByMarketplace(MP_CustomerMarketPlace customerMarketPlace)
        {
            return GetAll().FirstOrDefault(x => x.MarketPlace.Id == customerMarketPlace.Id);
        }

        public IEnumerable<Askville> GetAskvilleByCustomerId(int customerId)
        {
            return GetAll().Where(x => x.MarketPlace.Customer.Id == customerId);
        }
    }
}
