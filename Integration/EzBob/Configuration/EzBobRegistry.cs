namespace EzBob.Configuration
{
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using AmazonServiceLib.Config;
	using CommonLib;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using MailApi;
	using PayPalServiceLib;
	using PayPalServiceLib.Common;
	using TeraPeakServiceLib;
	using eBayLib.Config;
	using Scorto.Configuration;
	using StructureMap.Configuration.DSL;

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
			For<ILoanRepository>().Use<LoanRepository>();
			For<ILoanScheduleRepository>().Use<LoanScheduleRepository>();
			For<ILoanHistoryRepository>().Use<LoanHistoryRepository>();
		}
	}
}
