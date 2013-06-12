using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.PayPal;
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