using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Repository
{
    public class ServiceLogRepository : NHibernateRepositoryBase<MP_ServiceLog>
    {
        public ServiceLogRepository(ISession session) : base(session)
        {
        }
        public IEnumerable<MP_ServiceLog> GetBuCustomer(Customer customer)
        {
            return GetAll().Where(x => x.Customer.Id == customer.Id).ToFuture();
        }

		public MP_ServiceLog GetFirst()
		{
			return GetAll().FirstOrDefault();
		}
    }
}