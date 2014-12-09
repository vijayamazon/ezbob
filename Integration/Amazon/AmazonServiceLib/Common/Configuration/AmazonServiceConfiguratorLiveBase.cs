using System.Security.Policy;

namespace EzBob.AmazonServiceLib.Common.Configuration
{
	internal abstract class AmazonServiceConfiguratorLiveBase
	{
		protected AmazonServiceConfiguratorLiveBase( AmazonApiType apiType, AmazonServiceCountry serviceCountry, AmazonDeveloperAccessInfo accessInfo, AmazonApplicationInfo applicationInfo )
		{
			AccessInfo = accessInfo;
			ApplicationInfo = applicationInfo;
			var urlFactory  = AmazonServiceUrlHelper.CreateFactory(apiType);
			ServiceUrl = urlFactory.Create(serviceCountry);
		}

		protected Url ServiceUrl { get; private set; }
		protected AmazonDeveloperAccessInfo AccessInfo { get; private set; }
		protected AmazonApplicationInfo ApplicationInfo { get; private set; }

	}

}
