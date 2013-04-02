using MarketplaceWebServiceOrders.MarketplaceWebServiceOrders;

namespace EzBob.AmazonServiceLib.Orders.Configurator
{
	public interface IAmazonServiceOrdersConfigurator
	{
		IMarketplaceWebServiceOrders AmazonService { get; }
	}
}