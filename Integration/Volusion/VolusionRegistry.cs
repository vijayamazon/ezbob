using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using StructureMap.Configuration.DSL;

namespace Integration.Volusion {
	public class PluginRegistry : Registry {
		public PluginRegistry() {
			var oVsi = new VolusionServiceInfo();

			For<IMarketplaceType>().Use<VolusionDatabaseMarketPlace>().Named(oVsi.DisplayName);
			For<DatabaseMarketplaceBase<VolusionDatabaseFunctionType>>().Use<VolusionDatabaseMarketPlace>();
			For<IMarketplaceRetrieveDataHelper>().Use<VolusionRetriveDataHelper>().Named(oVsi.DisplayName);
		} // constructor
	} // class PluginRegistry
} // namespace