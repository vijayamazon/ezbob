using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using StructureMap.Configuration.DSL;

namespace CompanyFiles {
	public class PluginRegistry : Registry {
		public PluginRegistry() {
			For<IMarketplaceType>().Use<CompanyFilesDatabaseMarketPlace>().Named("CompanyFiles");
			For<IMarketplaceRetrieveDataHelper>().Use<CompanyFilesRetriveDataHelper>().Named("CompanyFiles");
		}
	}
}