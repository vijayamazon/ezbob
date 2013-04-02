using MarketplaceWebServiceProducts;

namespace EzBob.AmazonServiceLib.Products.Configurator
{
	public interface IAmazonServiceProductsConfigurator
	{
		IMarketplaceWebServiceProducts AmazonService { get; }
	}
}
