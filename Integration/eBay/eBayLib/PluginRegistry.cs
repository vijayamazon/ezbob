using EZBob.DatabaseLib.Common;
namespace EzBob.eBayLib
{
	using EZBob.DatabaseLib.DatabaseWrapper;
	using eBayDbLib;
	using Config;
	using eBayServiceLib;
	using StructureMap.Configuration.DSL;

	public class PluginRegistry : Registry
	{
		public PluginRegistry()
		{
		    var ebay = new eBayServiceInfo();
            For<IMarketplaceType>().Use<eBayDatabaseMarketPlace>().Named(ebay.DisplayName);
            For<DatabaseMarketplaceBase<eBayDatabaseFunctionType>>().Use<eBayDatabaseMarketPlace>();
            For<IEbayMarketplaceTypeConnection>().Use(new EbayMarketplaceTypeConnection());
			For<IEbayMarketplaceSettings>().Use( new EbayMarketPlaceTypeSettings() );
            For<IMarketplaceRetrieveDataHelper>().Use<eBayRetriveDataHelper>().Named(ebay.DisplayName);
		}
	}
}