using MarketplaceWebService;

namespace EzBob.AmazonServiceLib.MarketWebService.Configurator
{
	public interface IAmazonServiceReportsConfigurator
	{
		IMarketplaceWebService AmazonService { get; }
	}
}