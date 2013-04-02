using MarketplaceWebService;
using MarketplaceWebService.Mock;

namespace EzBob.AmazonServiceLib.MarketWebService.Configurator
{
	internal class AmazonWebServiceConfiguratorMock : IAmazonServiceReportsConfigurator
	{
		public IMarketplaceWebService AmazonService
		{
			get { return new MarketplaceWebServiceMock(); }
		}
	}
}