using FBAInventoryServiceMWS.Service;

namespace EzBob.AmazonServiceLib.Inventory.Configurator
{
	public interface IAmazonServiceInventoryConfigurator
	{
		IFbaInventoryServiceMws AmazonService { get; }
	}
}