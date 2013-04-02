using System;
using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.Products.Configurator
{
	public class AmazonServiceProductsConfiguratorFactory
	{
		public static IAmazonServiceProductsConfigurator Create( AmazonServiceConnectionInfo connectionInfo )
		{
			switch ( connectionInfo.ServiceType )
			{
				case AmazonServiceType.Live:
					return new AmazonServiceProductsConfiguratorLive( AmazonApiType.Products, connectionInfo.MarketCountry, connectionInfo.AccessInfo, connectionInfo.ApplicationInfo );

				case AmazonServiceType.Mock:
					return new AmazonServiceProductsConfiguratorMock();

				default:
					throw new NotImplementedException();
			}
		}
	}
}