namespace YodleeLib.connector {
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using StructureMap.Configuration.DSL;

	public class PluginRegistry : Registry {
		public PluginRegistry() {
			For<IMarketplaceType>().Use<YodleeDatabaseMarketPlace>().Named("Yodlee");
			For<IMarketplaceRetrieveDataHelper>().Use<YodleeRetriveDataHelper>().Named("Yodlee");
		}
	}
}