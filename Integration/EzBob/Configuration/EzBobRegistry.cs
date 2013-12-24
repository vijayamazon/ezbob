﻿namespace EzBob.Configuration
{
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using AmazonServiceLib.Config;
	using CommonLib;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using MailApi;
	using PayPalServiceLib;
	using PayPalServiceLib.Common;
	using TeraPeakServiceLib;
	using eBayLib.Config;
	using Scorto.Configuration;
	using StructureMap.Configuration.DSL;
	using FreeAgent.Config;
	using Sage.Config;

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
				For<IFreeAgentConfig>().Singleton().Use(ezBobConfigRoot.FreeAgentConfig);
				For<ISageConfig>().Singleton().Use(ezBobConfigRoot.SageConfig);
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
			For<ICustomerStatusesRepository>().Use<CustomerStatusesRepository>();
			For<IApprovalsWithoutAMLRepository>().Use<ApprovalsWithoutAMLRepository>();
			For<IMail>().Use<Mail>();
			var bobconfig = EnvironmentConfiguration.Configuration.GetCurrentConfiguration<ConfigurationRootBob>();
			For<IMandrillConfig>().Use(bobconfig.MandrillConfig);
			For<ILoanRepository>().Use<LoanRepository>();
		}
	}
}
