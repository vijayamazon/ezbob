using EzBob.eBayServiceLib.Common;

namespace EzBob.eBayLib.Config
{
	using ConfigManager;

	public class EbayMarketplaceTypeConnection : EbayMarketplaceTypeConnectionBase
	{
		public EbayMarketplaceTypeConnection()
		{
			string serviceTypeStr = CurrentValues.Instance.EbayServiceType;
			ServiceType = serviceTypeStr == "Production" ? ServiceEndPointType.Production : ServiceEndPointType.Sandbox;
			DevId = CurrentValues.Instance.EbayDevId;
			AppId = CurrentValues.Instance.EbayAppId;
			CertId = CurrentValues.Instance.EbayCertId;
			RuName = CurrentValues.Instance.EbayRuName;
			DownloadCategories = true;
		}
	}
}