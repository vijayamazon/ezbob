namespace EzBob.AmazonServiceLib.Orders.Configurator
{
	using MarketplaceWebServiceOrders.Mock;

	internal class AmazonServiceOrdersConfiguratorMock : IAmazonServiceOrdersConfigurator
	{
		public MarketplaceWebServiceOrders.MarketplaceWebServiceOrders AmazonService
		{
			get { return new MarketplaceWebServiceOrdersMock(); }
		}
	}
}