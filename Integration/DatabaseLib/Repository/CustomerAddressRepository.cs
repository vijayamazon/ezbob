using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;

	public interface ICustomerAddressRepository : IRepository<CustomerAddress>
    {

    }
    public class CustomerAddressRepository : NHibernateRepositoryBase<CustomerAddress>, ICustomerAddressRepository
    {
        public CustomerAddressRepository(ISession session) : base(session)
        {
        }
    }
}
