using ApplicationMng.Repository;
using StructureMap.Configuration.DSL;

namespace EZBob.DatabaseLib
{
    public class RepositoryRegistry : Registry
    {
        public RepositoryRegistry()
        {
            For<ICustomerAddressRepository>().Use<CustomerAddressRepository>();
        }
    }
}