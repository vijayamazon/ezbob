using EzBob.eBayServiceLib.Common;

namespace EzBob.eBayLib.Config
{
	public class EbayMarketplaceTypeConnection : EbayMarketplaceTypeConnectionBase
	{
		public EbayMarketplaceTypeConnection()
		{
			ServiceType = ServiceEndPointType.Production;
			DevId = @"87cffac6-4c2c-4352-bc88-7cdba02b8085" ;
			AppId = "test8f473-e17f-4c46-8ee7-22418d11dd2";
			CertId = @"aea6ac5f-7a80-48ff-820d-355f1979a6e5";
			RuName = @"test-test8f473-e17f--xjwup";
			DownloadCategories = true;
		}
	}
}