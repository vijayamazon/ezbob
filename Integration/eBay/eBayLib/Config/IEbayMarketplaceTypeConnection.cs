using EzBob.eBayServiceLib.Common;

namespace EzBob.eBayLib.Config
{
	public interface IEbayMarketplaceTypeConnection
	{
        ServiceEndPointType ServiceType { get; }
		string DevId { get; }
		string AppId { get; }
		string CertId { get; }
	}
}