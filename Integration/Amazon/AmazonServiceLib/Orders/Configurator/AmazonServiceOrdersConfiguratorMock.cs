using MarketplaceWebServiceOrders.MarketplaceWebServiceOrders;
using MarketplaceWebServiceOrders.MarketplaceWebServiceOrders.Mock;

namespace EzBob.AmazonServiceLib.Orders.Configurator
{
	internal class AmazonServiceOrdersConfiguratorMock : IAmazonServiceOrdersConfigurator
	{
		public IMarketplaceWebServiceOrders AmazonService
		{
			get { return new MarketplaceWebServiceOrdersMock(); }
		}
	}
}