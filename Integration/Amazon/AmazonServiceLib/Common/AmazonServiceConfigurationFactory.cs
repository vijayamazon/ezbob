namespace EzBob.AmazonServiceLib.Common
{
	using MarketWebService.Configurator;
	using Orders.Configurator;
	using Products.Configurator;

	public static class AmazonServiceConfigurationFactory
	{
		public static IAmazonServiceOrdersConfigurator CreateServiceOrdersConfigurator( AmazonServiceConnectionInfo connectionInfo )
		{
			return AmazonServiceOrdersConfiguratorFactory.Create( connectionInfo );
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