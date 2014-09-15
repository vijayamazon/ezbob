namespace EzBob.AmazonServiceLib.Orders.Configurator
{
	public interface IAmazonServiceOrdersConfigurator
	{
		MarketplaceWebServiceOrders.MarketplaceWebServiceOrders AmazonService { get; }
	}
}