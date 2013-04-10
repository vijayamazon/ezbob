using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.CommonLib;
using StructureMap.Configuration.DSL;

namespace EKM
{
    public class PluginRegistry : Registry
    {
        public PluginRegistry()
        {
            For<IMarketplaceType>().Use<EkmDatabaseMarketPlace>().Named("EKM");
            For<DatabaseMarketplaceBase<EkmDatabaseFunctionType>>().Use<EkmDatabaseMarketPlace>();
            For<IMarketplaceRetrieveDataHelper>().Use<EkmRetriveDataHelper>().Named("EKM");
        }
    }
}