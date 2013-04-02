using MarketplaceWebServiceProducts;
using MarketplaceWebServiceProducts.Mock;

namespace EzBob.AmazonServiceLib.Products.Configurator
{
	internal class AmazonServiceProductsConfiguratorMock : IAmazonServiceProductsConfigurator
	{
		public IMarketplaceWebServiceProducts AmazonService
		{
			get { return new MarketplaceWebServiceProductsMock(); }
		}
	}
}