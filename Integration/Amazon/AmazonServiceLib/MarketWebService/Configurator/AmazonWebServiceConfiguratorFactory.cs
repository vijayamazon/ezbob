using System;
using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.MarketWebService.Configurator
{
	public class AmazonWebServiceConfiguratorFactory
	{
		public static IAmazonServiceReportsConfigurator Create( AmazonServiceConnectionInfo connectionInfo )
		{
			switch ( connectionInfo.ServiceType )
			{
				case AmazonServiceType.Live:
					return new AmazonWebServiceConfiguratorLive( AmazonApiType.WebService, connectionInfo.MarketCountry, connectionInfo.AccessInfo, connectionInfo.ApplicationInfo );

				case AmazonServiceType.Mock:
					return new AmazonWebServiceConfiguratorMock();

				default:
					throw new NotImplementedException();
			}
		}
	}
}