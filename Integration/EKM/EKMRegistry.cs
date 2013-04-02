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
            For<IDatabaseMarketplace>().Use<EkmDatabaseMarketPlace>().Named("EKM");
            For<DatabaseMarketplaceBase<EKMDatabaseFunctionType>>().Use<EkmDatabaseMarketPlace>();
            For<IMarketplaceRetrieveDataHelper>().Use<EkmRetriveDataHelper>().Named("EKM");
        }
    }
}