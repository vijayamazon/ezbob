using System.Diagnostics;
using EzBob.AmazonServiceLib.Common;
using EzBob.AmazonServiceLib.Common.Configuration;
using FBAInventoryServiceMWS.Service;

namespace EzBob.AmazonServiceLib.Inventory.Configurator
{
	internal class AmazonServiceInventoryConfiguratorLive : AmazonServiceConfiguratorLiveBase, IAmazonServiceInventoryConfigurator
	{
		private IFbaInventoryServiceMws _AmazonService;

		public AmazonServiceInventoryConfiguratorLive(AmazonApiType apiType, AmazonServiceCountry serviceCountry, AmazonDeveloperAccessInfo accessInfo, AmazonApplicationInfo applicationInfo) 
			: base(apiType, serviceCountry, accessInfo, applicationInfo)
		{
			Debug.Assert( apiType == AmazonApiType.Inventory );
		}

		private IFbaInventoryServiceMws CreateService()
		{
			/************************************************************************
				 * Access Key ID and Secret Access Key ID
			***********************************************************************/
			string accessKeyId = base.AccessInfo.KeyId;
			string secretAccessKey = base.AccessInfo.SecretKeyId;

			/************************************************************************
             * The application name and version are included in each MWS call's
             * HTTP User-Agent field. These are required fields.
             ***********************************************************************/
			string applicationName = base.ApplicationInfo.Name;
			string applicationVersion = base.ApplicationInfo.Version;
			/************************************************************************
			* Uncomment to try advanced configuration options. Available options are:
			*
			*  - Proxy Host and Proxy Port
			*  - MWS Service endpoint URL
			*  - User Agent String to be sent to FBA Inventory Service MWS  service
			*
			***********************************************************************/
			//config.ProxyHost = "https://PROXY_URL";
			//config.ProxyPort = 9090;
			//
			// ProxyPort=-1 ; MaxErrorRetry=3

			/************************************************************************
			* Instantiate Implementation of FBA Inventory Service MWS 
			***********************************************************************/
			var config = new FBAInventoryServiceMWSConfig
			{
				ServiceURL = base.ServiceUrl.Value
			};

			return new FBAInventoryServiceMWSClient( accessKeyId, secretAccessKey, applicationName, applicationVersion, config );
		}

		public IFbaInventoryServiceMws AmazonService
		{
			get { return _AmazonService ?? (_AmazonService = CreateService()); }
		}
	}
}