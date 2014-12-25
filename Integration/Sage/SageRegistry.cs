namespace Sage {
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using StructureMap.Configuration.DSL;

	public class PluginRegistry : Registry {
		public PluginRegistry() {
			For<IMarketplaceType>().Use<SageDatabaseMarketPlace>().Named("Sage");
			For<IMarketplaceRetrieveDataHelper>().Use<SageRetrieveDataHelper>().Named("Sage");
		}
	}
}