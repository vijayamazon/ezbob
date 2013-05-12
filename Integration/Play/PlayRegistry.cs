using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using StructureMap.Configuration.DSL;

namespace Integration.Play {
	public class PluginRegistry : Registry {
		public PluginRegistry() {
			var oPsi = new PlayServiceInfo();

			For<IMarketplaceType>().Use<PlayDatabaseMarketPlace>().Named(oPsi.DisplayName);
			For<DatabaseMarketplaceBase<PlayDatabaseFunctionType>>().Use<PlayDatabaseMarketPlace>();
			For<IMarketplaceRetrieveDataHelper>().Use<PlayRetriveDataHelper>().Named(oPsi.DisplayName);
		} // constructor
	} // class PluginRegistry
} // namespace