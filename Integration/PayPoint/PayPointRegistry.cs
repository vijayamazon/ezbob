namespace PayPoint {
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using StructureMap.Configuration.DSL;

	public class PluginRegistry : Registry {
		public PluginRegistry() {
			For<IMarketplaceType>().Use<PayPointDatabaseMarketPlace>().Named("PayPoint");
			For<IMarketplaceRetrieveDataHelper>().Use<PayPointRetrieveDataHelper>().Named("PayPoint");
		}
	}
}