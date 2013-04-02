using EzBob.AmazonServiceLib.Inventory.Configurator;
using EzBob.AmazonServiceLib.MarketWebService.Configurator;
using EzBob.AmazonServiceLib.Orders.Configurator;
using EzBob.AmazonServiceLib.Products.Configurator;

namespace EzBob.AmazonServiceLib.Common
{
	public static class AmazonServiceConfigurationFactory
	{
		public static IAmazonServiceOrdersConfigurator CreateServiceOrdersConfigurator( AmazonServiceConnectionInfo connectionInfo )
		{
			return AmazonServiceOrdersConfiguratorFactory.Create( connectionInfo );
		}

		public static IAmazonServiceInventoryConfigurator CreateServiceInventoryConfigurator( AmazonServiceConnectionInfo connectionInfo )
		{
			return AmazonServiceInventoryConfiguratorFactory.Create( connectionInfo );
		}

		public static IAmazonServiceReportsConfigurator CreateServiceReportsConfigurator( AmazonServiceConnectionInfo connectionInfo )
		{
			return AmazonWebServiceConfiguratorFactory.Create( connectionInfo );
		}

		public static IAmazonServiceProductsConfigurator CreateServiceProductsConfigurator( AmazonServiceConnectionInfo connectionInfo )
		{
			return AmazonServiceProductsConfiguratorFactory.Create( connectionInfo );
		}
	}
}