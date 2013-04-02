using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
    public class PostcodeServiceLogRepository : NHibernateRepositoryBase<PostcodeServiceLog>
    {
        public PostcodeServiceLogRepository(ISession session) : base(session)
        {
        }

        public IEnumerable<PostcodeServiceLog> GetByCustomer(Customer customer)
        {
            return GetAll().Where(x => x.Customer.Id == customer.Id).ToFuture();
        }
    }
}
