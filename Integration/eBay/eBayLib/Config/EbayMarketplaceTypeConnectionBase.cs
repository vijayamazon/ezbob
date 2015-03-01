using EzBob.eBayServiceLib.Common;

namespace EzBob.eBayLib.Config
{
	public abstract class EbayMarketplaceTypeConnectionBase : IEbayMarketplaceTypeConnection
	{
		public ServiceEndPointType ServiceType { get; set; }
		public string DevId { get; set; }
		public string AppId { get; set; }
		public string CertId { get; set; }
		public bool DownloadCategories { get; set; }
	}
}