namespace EKM {
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using StructureMap.Configuration.DSL;

	public class PluginRegistry : Registry {
		public PluginRegistry() {
			For<IMarketplaceType>().Use<EkmDatabaseMarketPlace>().Named("EKM");
			For<IMarketplaceRetrieveDataHelper>().Use<EkmRetriveDataHelper>().Named("EKM");
		}
	}
}