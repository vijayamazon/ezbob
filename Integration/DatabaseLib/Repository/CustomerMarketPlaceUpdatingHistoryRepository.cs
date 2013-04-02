using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	public class CustomerMarketPlaceUpdatingHistoryRepository :NHibernateRepositoryBase<MP_CustomerMarketplaceUpdatingHistory>
	{
		public CustomerMarketPlaceUpdatingHistoryRepository(ISession session) 
			: base(session)
		{
		}

	    public IQueryable<MP_CustomerMarketplaceUpdatingHistory> GetByCustomer(Customer customer)
	    {
	        return GetAll().Where(x => x.CustomerMarketPlace.Customer.Id == customer.Id).OrderBy(x => x.UpdatingStart);
	    }
	}
}