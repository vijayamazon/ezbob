using EzBob.CommonLib;

namespace EzBob.AmazonServiceLib.Config
{
	public interface IAmazonMarketplaceSettings
	{
		ErrorRetryingInfo ErrorRetryingInfo { get; }
	}
}