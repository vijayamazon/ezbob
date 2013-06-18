using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.CommonLib;
using Integration.ChannelGrabberConfig;
using StructureMap.Configuration.DSL;
using Integration.ChannelGrabberFrontend;

namespace Integration.Shopify {
	public class ShopifyServiceInfo : ServiceInfo, IMarketplaceServiceInfo {
		public const string VendorName = "Shopify";

		public ShopifyServiceInfo() {
			Load(VendorName);
		} // constructor
	} // ShopifyServiceInfo

	public class PluginRegistry : Registry {
		public PluginRegistry() {
			For<IMarketplaceType>().Use<DatabaseMarketPlace<ShopifyServiceInfo>>().Named(ShopifyServiceInfo.VendorName);
			For<DatabaseMarketplaceBase<FunctionType>>().Use<DatabaseMarketPlace<ShopifyServiceInfo>>();
			For<IMarketplaceRetrieveDataHelper>().Use<RetriveDataHelper>().Named(ShopifyServiceInfo.VendorName);
		} // constructor
	} // class PluginRegistry
} // namespace