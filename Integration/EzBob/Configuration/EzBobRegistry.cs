namespace EzBob.Configuration
{
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using AmazonServiceLib.Config;
	using CommonLib;
	using PayPalServiceLib;
	using PayPalServiceLib.Common;
	using TeraPeakServiceLib;
	using eBayLib.Config;
	using Scorto.Configuration;
	using StructureMap.Configuration.DSL;
	using FreeAgent.Config;
	using YodleeLib.config;

    public class EzBobRegistry : Registry
	{
		public EzBobRegistry()
		{
			var ezBobConfigRoot = EnvironmentConfiguration.Configuration.GetCurrentConfiguration<EzBobConfigRoot>();
			if ( ezBobConfigRoot != null )
			{
				For<IPayPalConfig>().Singleton().Use(ezBobConfigRoot.PayPalConfig);
				For<IPayPalMarketplaceSettings>().Singleton().Use( ezBobConfigRoot.PayPalSettings );
				For<IEbayMarketplaceTypeConnection>().Use( ezBobConfigRoot.eBayConfig);
				For<IEbayMarketplaceSettings>().Use( ezBobConfigRoot.eBaySettings );
				For<IAmazonMarketPlaceTypeConnection>().Use( ezBobConfigRoot.AmazonConfig);
				For<IAmazonMarketplaceSettings>().Use(ezBobConfigRoot.AmazonSetings);
				For<IYodleeMarketPlaceConfig>().Singleton().Use(ezBobConfigRoot.YodleeConfig);
				For<IFreeAgentConfig>().Singleton().Use(ezBobConfigRoot.FreeAgentConfig);
			}
			var teraPeakConfigRoot = EnvironmentConfiguration.Configuration.GetCurrentConfiguration<TeraPeakConfigRoot>();
			if ( ezBobConfigRoot != null )
			{
				For<ITeraPeakCredentionProvider>().Singleton().Use( teraPeakConfigRoot.TeraPeakCredentionProvider );
				For<ITeraPeakConnectionProvider>().Singleton().Use( teraPeakConfigRoot.TeraPeakCredentionProvider );
			}

			
			For<IServiceEndPointFactory>().Use( new ServiceEndPointFactory() );

			For<IDatabaseDataHelper>().Use<DatabaseDataHelper>();
            For<IBugRepository>().Use<BugRepository>();
		}
	}
}
