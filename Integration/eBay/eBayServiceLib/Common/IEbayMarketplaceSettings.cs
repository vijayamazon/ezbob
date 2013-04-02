using EzBob.CommonLib;

namespace EzBob.eBayLib.Config
{
	public interface IEbayMarketplaceSettings
	{
		bool DownloadCategories { get; }
		ErrorRetryingInfo ErrorRetryingInfo { get; }
		bool OrdersFromTeraPeakOnly { get; }

		bool DisableUpdate { get; }
	}
}