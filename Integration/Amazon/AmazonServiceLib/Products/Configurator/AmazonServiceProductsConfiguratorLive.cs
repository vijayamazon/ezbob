using System.Diagnostics;
using EzBob.AmazonServiceLib.Common;
using EzBob.AmazonServiceLib.Common.Configuration;
using MarketplaceWebServiceProducts;

namespace EzBob.AmazonServiceLib.Products.Configurator
{
	internal class AmazonServiceProductsConfiguratorLive : AmazonServiceConfiguratorLiveBase, IAmazonServiceProductsConfigurator
	{
		private IMarketplaceWebServiceProducts _AmazonService;

		public AmazonServiceProductsConfiguratorLive(AmazonApiType apiType, AmazonServiceCountry serviceCountry, AmazonDeveloperAccessInfo accessInfo, AmazonApplicationInfo applicationInfo) 
			: base(apiType, serviceCountry, accessInfo, applicationInfo)
		{
			Debug.Assert( apiType == AmazonApiType.Products );
		}

		public IMarketplaceWebServiceProducts AmazonService
		{
			get { return _AmazonService ?? ( _AmazonService = CreateService() ); }
		}

		private IMarketplaceWebServiceProducts CreateService()
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

			var config = new MarketplaceWebServiceProductsConfig
			{
				ServiceURL = base.ServiceUrl.Value
			};
			/************************************************************************
            * Instantiate  Implementation of Marketplace Web Service Orders 
            ***********************************************************************/
			return new MarketplaceWebServiceProductsClient( applicationName, applicationVersion, accessKeyId, secretAccessKey, config );
		}
	}
}