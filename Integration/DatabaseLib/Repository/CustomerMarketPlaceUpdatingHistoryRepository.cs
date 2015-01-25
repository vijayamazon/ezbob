namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;

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

		public IQueryable<MP_CustomerMarketplaceUpdatingHistory> GetByMarketplaceId(int marketplaceId)
		{
			return GetAll().Where(x => x.CustomerMarketPlace.Id == marketplaceId).OrderByDescending(x => x.UpdatingStart);
		}

		public IQueryable<MP_CustomerMarketplaceUpdatingHistory> GetByCustomerId(int customerID)
		{
			return GetAll().Where(x => x.CustomerMarketPlace.Customer.Id == customerID);
		}

	}
}