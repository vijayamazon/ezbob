namespace EzBob.Models.Marketplaces.Builders {
	using EZBob.DatabaseLib.Model.Database;
	using StructureMap.Configuration.DSL;

	public class MPRegistry : Registry {
		public MPRegistry() {
			For<IMarketplaceModelBuilder>().Use<MarketplaceModelBuilder>().Named("DEFAULT");
			For<IMarketplaceModelBuilder>().Use<PayPalMarketplaceModelBuilder>().Named(typeof(PayPalMarketPlaceType).ToString());
			For<IMarketplaceModelBuilder>().Use<AmazonMarketplaceModelBuilder>().Named(typeof(AmazonMarketPlaceType).ToString());
			For<IMarketplaceModelBuilder>().Use<EBayMarketplaceModelBuilder>().Named(typeof(eBayMarketPlaceType).ToString());
			For<IMarketplaceModelBuilder>().Use<PayPointMarketplaceModelBuilder>().Named(typeof(PayPointMarketPlaceType).ToString());
			For<IMarketplaceModelBuilder>().Use<YodleeMarketplaceModelBuilder>().Named(typeof(YodleeMarketPlaceType).ToString());
			For<IMarketplaceModelBuilder>().Use<FreeAgentMarketplaceModelBuilder>().Named(typeof(FreeAgentMarketPlaceType).ToString());
			For<IMarketplaceModelBuilder>().Use<SageMarketplaceModelBuilder>().Named(typeof(SageMarketPlaceType).ToString());
			For<IMarketplaceModelBuilder>().Use<EkmMarketplaceModelBuilder>().Named(typeof(EKMMarketPlaceType).ToString());
			For<IMarketplaceModelBuilder>().Use<CompanyFilesMarketplaceModelBuilder>().Named(typeof(CompanyFilesMarketPlaceType).ToString());
			For<IMarketplaceModelBuilder>().Use<ChannelGrabberMarketplaceModelBuilder>().Named(typeof(ChannelGrabberMarketPlaceType).ToString());
		} // constructor
	} // class MPRegistry
} // namespace
