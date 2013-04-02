using EzBob.CommonLib;

namespace EzBob.eBayLib.Config
{
	class EbayMarketPlaceTypeSettings : EbayMarketPlaceTypeSettingsBase
	{
		public override bool DownloadCategories
		{
			get { return true; }
		}

		public override ErrorRetryingInfo ErrorRetryingInfo
		{
			get 
			{
				return new ErrorRetryingInfo
				{
					Info = new[]
					{
						new ErrorRetryingItemInfo
						{
							CountRequestsExpectError =5,
							TimeOutAfterRetryingExpiredInMinutes = 10
						}
					}
				};
			}
		}

		public override bool OrdersFromTeraPeakOnly
		{
			get { return true; }
		}

		public override bool DisableUpdate
		{
			get { return false; }
		}
	}
}