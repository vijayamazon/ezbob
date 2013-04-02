using System;
using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.Inventory.Configurator
{
	public class AmazonServiceInventoryConfiguratorFactory
	{
		public static IAmazonServiceInventoryConfigurator Create( AmazonServiceConnectionInfo connectionInfo )
		{
			switch ( connectionInfo.ServiceType )
			{
				case AmazonServiceType.Live:
					return new AmazonServiceInventoryConfiguratorLive( AmazonApiType.Inventory, connectionInfo.MarketCountry, connectionInfo.AccessInfo, connectionInfo.ApplicationInfo );


				case AmazonServiceType.Mock:
					return new AmazonServiceInventoryConfiguratorMock();
				default:
					throw new NotImplementedException();
			}
		}
	}
}