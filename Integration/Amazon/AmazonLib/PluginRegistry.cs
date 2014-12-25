namespace EzBob.AmazonLib {
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EzBob.AmazonServiceLib;
	using StructureMap.Configuration.DSL;

	public class PluginRegistry : Registry {
		public PluginRegistry() {
			var amazon = new AmazonServiceInfo();
			For<IMarketplaceType>().Use<AmazonDatabaseMarketPlace>().Named(amazon.DisplayName);
			For<IMarketplaceRetrieveDataHelper>().Use<AmazonRetriveDataHelper>().Named(amazon.DisplayName);
		}
	}
}