using EzBob.CommonLib;

namespace EzBob.eBayLib.Config
{
	public abstract class EbayMarketPlaceTypeSettingsBase : IEbayMarketplaceSettings
	{
		public abstract bool DownloadCategories { get; }
		public abstract ErrorRetryingInfo ErrorRetryingInfo { get; }
		public abstract bool OrdersFromTeraPeakOnly { get;  }
		public abstract bool DisableUpdate { get; }
	}
}