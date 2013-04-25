using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.AmazonDbLib;
using EzBob.AmazonServiceLib;
using EzBob.AmazonServiceLib.Config;
using StructureMap.Configuration.DSL;

namespace EzBob.AmazonLib
{
	public class PluginRegistry : Registry
	{
		public PluginRegistry()
		{
		    var amazon = new AmazonServiceInfo();
            For<IMarketplaceType>().Use<AmazonDatabaseMarketPlace>().Named(amazon.DisplayName);
            For<DatabaseMarketplaceBase<AmazonDatabaseFunctionType>>().Use<AmazonDatabaseMarketPlace>();
            For<IMarketplaceRetrieveDataHelper>().Use<AmazonRetriveDataHelper>().Named(amazon.DisplayName);
		}
	}
}