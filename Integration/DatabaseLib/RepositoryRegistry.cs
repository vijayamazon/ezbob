using ApplicationMng.Repository;
using EZBob.DatabaseLib.PyaPalDetails;
using StructureMap.Configuration.DSL;

namespace EZBob.DatabaseLib
{
    public class RepositoryRegistry : Registry
    {
        public RepositoryRegistry()
        {
            For<ICustomerAddressRepository>().Use<CustomerAddressRepository>();
            For<IPayPalDetailsRepository>().Use<PayPalDetailsRepository>();
        }
    }
}