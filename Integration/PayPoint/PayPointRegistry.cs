using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.CommonLib;
using StructureMap.Configuration.DSL;

namespace PayPoint
{
    public class PluginRegistry : Registry
    {
        public PluginRegistry()
        {
            For<IDatabaseMarketplace>().Use<PayPointDatabaseMarketPlace>().Named("PayPoint");
            For<DatabaseMarketplaceBase<PayPointDatabaseFunctionType>>().Use<PayPointDatabaseMarketPlace>();
            For<IMarketplaceRetrieveDataHelper>().Use<PayPointRetriveDataHelper>().Named("PayPoint");
        }
    }
}