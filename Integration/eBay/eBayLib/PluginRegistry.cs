namespace EzBob.eBayLib
{
	using EZBob.DatabaseLib.Common;
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
            For<IEbayMarketplaceTypeConnection>().Use<EbayMarketplaceTypeConnection>();
			For<IEbayMarketplaceSettings>().Use( new EbayMarketPlaceTypeSettings() );
            For<IMarketplaceRetrieveDataHelper>().Use<eBayRetriveDataHelper>().Named(ebay.DisplayName);
		}
	}
}