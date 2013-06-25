using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using Integration.ChannelGrabberConfig;
using StructureMap.Configuration.DSL;
using log4net;

namespace Integration.ChannelGrabberFrontend {
	public class PluginRegistry : Registry {
		public PluginRegistry() {
			ILog oLog = LogManager.GetLogger(typeof (PluginRegistry));

			oLog.Debug("Plugin registry for Channel Grabber DLL");

			Configuration.Instance.ForEachVendor( vi => {
				For<IMarketplaceType>().Use(new DatabaseMarketPlace(vi.Name)).Named(vi.Name);
				For<DatabaseMarketplaceBase<FunctionType>>().Use(new DatabaseMarketPlace(vi.Name));
				For<IMarketplaceRetrieveDataHelper>().Use<RetriveDataHelper>().Named(vi.Name);
				oLog.DebugFormat("Plugin registry done for IMarketplaceType and IMarketplaceRetrieveDataHelper with named instance {0}", vi.Name);
			});
		} // constructor
	} // class PluginRegistry
} // namespace