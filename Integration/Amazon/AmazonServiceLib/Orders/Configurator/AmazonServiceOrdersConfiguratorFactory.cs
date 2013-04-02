using System;
using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.Orders.Configurator
{
	public class AmazonServiceOrdersConfiguratorFactory
	{
		public static IAmazonServiceOrdersConfigurator Create( AmazonServiceConnectionInfo connectionInfo )
		{
			switch (connectionInfo.ServiceType)
			{
				case AmazonServiceType.Live:
					return new AmazonServiceOrdersConfiguratorLive( AmazonApiType.Orders, connectionInfo.MarketCountry, connectionInfo.AccessInfo, connectionInfo.ApplicationInfo );

				case AmazonServiceType.Mock:
					return new AmazonServiceOrdersConfiguratorMock();

				default:
					throw new NotImplementedException();
			}
		}
	}
}