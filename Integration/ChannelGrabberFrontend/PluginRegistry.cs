using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using Integration.ChannelGrabberConfig;
using StructureMap.Configuration.DSL;
using log4net;

namespace Integration.ChannelGrabberFrontend {
	public class PluginRegistry : Registry {
		public PluginRegistry() {
			Configuration.Instance.ForEachVendor( vi => {
				For<IMarketplaceType>().Use(new DatabaseMarketPlace(vi.Name)).Named(vi.Name);
				For<DatabaseMarketplaceBase<FunctionType>>().Use(new DatabaseMarketPlace(vi.Name));
				For<IMarketplaceRetrieveDataHelper>().Use<RetrieveDataHelper>().Named(vi.Name);
			});
		} // constructor
	} // class PluginRegistry
} // namespace