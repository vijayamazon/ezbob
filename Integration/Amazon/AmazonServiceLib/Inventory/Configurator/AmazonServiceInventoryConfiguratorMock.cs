using FBAInventoryServiceMWS.Service;
using FBAInventoryServiceMWS.Service.Mock;

namespace EzBob.AmazonServiceLib.Inventory.Configurator
{
	internal class AmazonServiceInventoryConfiguratorMock : IAmazonServiceInventoryConfigurator
	{
		public IFbaInventoryServiceMws AmazonService
		{
			get { return new FBAInventoryServiceMWSMock(); }
		}
	}
}