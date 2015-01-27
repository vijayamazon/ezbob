using System.Collections.Generic;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model;
using NHibernate;

namespace EZBob.DatabaseLib.Repository
{
	public interface ICustomerSessionsRepository : IRepository<CustomerSession>
	{
		IList<CustomerSession> GetSessionIpLogByCustomerId(int id);
		void AddSessionIpLog(CustomerSession cs);
	}

	public class CustomerSessionsRepository : NHibernateRepositoryBase<CustomerSession>, ICustomerSessionsRepository
	{
		public CustomerSessionsRepository(ISession session)
			: base(session)
		{
		}

		public IList<CustomerSession> GetSessionIpLogByCustomerId(int id)
		{
			return Session.QueryOver<CustomerSession>().Where(c => c.CustomerId == id).List<CustomerSession>();
		}
		public void AddSessionIpLog(CustomerSession cs)
		{
			Save(cs);
		}
	}
}
