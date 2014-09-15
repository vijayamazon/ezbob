using System.Diagnostics;
using EzBob.AmazonServiceLib.Common;
using EzBob.AmazonServiceLib.Common.Configuration;
using MarketplaceWebService;

namespace EzBob.AmazonServiceLib.MarketWebService.Configurator {
	internal class AmazonWebServiceConfiguratorLive : AmazonServiceConfiguratorLiveBase, IAmazonServiceReportsConfigurator {
		private IMarketplaceWebService _AmazonService;

		public AmazonWebServiceConfiguratorLive(AmazonApiType apiType, 
			AmazonServiceCountry serviceCountry, 
			AmazonDeveloperAccessInfo accessInfo, 
			AmazonApplicationInfo applicationInfo)
			: base(apiType, serviceCountry, accessInfo, applicationInfo) {
			Debug.Assert(apiType == AmazonApiType.WebService);
		}

		public IMarketplaceWebService AmazonService {
			get { return _AmazonService ?? (_AmazonService = CreateService()); }
		}

		private IMarketplaceWebService CreateService() {
			/**********************************************************************
				 * Access Key ID and Secret Access Key ID
			***********************************************************************/
			string accessKeyId = base.AccessInfo.KeyId;
			string secretAccessKey = base.AccessInfo.SecretKeyId;

			/**********************************************************************
			* The application name and version are included in each MWS call's
			* HTTP User-Agent field. These are required fields.
			***********************************************************************/
			string applicationName = base.ApplicationInfo.Name;
			string applicationVersion = base.ApplicationInfo.Version;

			/**********************************************************************
			* Instantiate  Implementation of Marketplace Web Service 
			***********************************************************************/
			var config = new MarketplaceWebServiceConfig {
				ServiceURL = base.ServiceUrl.Value
			};

			return new MarketplaceWebServiceClient(accessKeyId, secretAccessKey, applicationName, applicationVersion, config);
		}
	}
}