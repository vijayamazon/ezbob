using EzBob.CommonLib;
using EzBob.PayPalServiceLib;

namespace EzBob.PayPal
{
	public class PayPalMarketplaceSettingsHardcode : IPayPalMarketplaceSettings
	{
		public ErrorRetryingInfo ErrorRetryingInfo
		{
			get
			{
				return new ErrorRetryingInfo()
					       {
						       Info = new[]
							              {
								              new ErrorRetryingItemInfo( 1, 2, 12 * 60),
								              new ErrorRetryingItemInfo( 2, 2, 18 * 60),
								              new ErrorRetryingItemInfo( 3, 2, 0),
							              },

					       };
			}
		}

		public int MonthsBack
		{
			get { return 12; }
		}

		public int MaxMonthsPerRequest
		{
			get { return 3; }
		}

		public int OpenTimeOutInMinutes
		{
			get { return 1; }
		}

		public int SendTimeoutInMinutes
		{
			get { return 1; }
		}

	    public bool EnableCategories
	    {
	        get { return true; }
	    }
	}
}