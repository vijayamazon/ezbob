using ApplicationMng.Model;
using NHibernate;

namespace ApplicationMng.Repository
{
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
