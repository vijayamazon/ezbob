using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.CommonLib;
using StructureMap.Configuration.DSL;

namespace YodleeLib.connector
{
    public class PluginRegistry : Registry
    {
        public PluginRegistry()
        {
            For<IMarketplaceType>().Use<YodleeDatabaseMarketPlace>().Named("Yodlee");
            For<DatabaseMarketplaceBase<YodleeDatabaseFunctionType>>().Use<YodleeDatabaseMarketPlace>();
            For<IMarketplaceRetrieveDataHelper>().Use<YodleeRetriveDataHelper>().Named("Yodlee");
        }
    }
}