using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.PayPal;
using EzBob.PayPalServiceLib;
using EzBob.eBayDbLib;
using EzBob.eBayLib.Config;
using EzBob.eBayServiceLib;
using StructureMap.Configuration.DSL;

namespace EzBob.eBayLib
{
	public class PluginRegistry : Registry
	{
		public PluginRegistry()
		{
		    var ebay = new eBayServiceInfo();
            For<IDatabaseMarketplace>().Use<eBayDatabaseMarketPlace>().Named(ebay.DisplayName);
            For<DatabaseMarketplaceBase<eBayDatabaseFunctionType>>().Use<eBayDatabaseMarketPlace>();
            For<IEbayMarketplaceTypeConnection>().Use(new EbayMarketplaceTypeConnection());
			For<IEbayMarketplaceSettings>().Use( new EbayMarketPlaceTypeSettings() );
			For<IPayPalMarketplaceSettings>().Use( new PayPalMarketplaceSettingsHardcode() );
            For<IMarketplaceRetrieveDataHelper>().Use<eBayRetriveDataHelper>().Named(ebay.DisplayName);
		}
	}
}