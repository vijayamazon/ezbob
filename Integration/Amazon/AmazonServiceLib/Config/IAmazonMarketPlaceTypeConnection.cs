using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.Config
{
	public interface IAmazonMarketPlaceTypeConnection
	{
		string KeyId { get; }
		string SecretKeyId { get; }
		string AskvilleAmazonLogin { get; }
        string AskvilleAmazonPass { get; }
        AmazonServiceType ServiceType { get; }
        AmazonServiceCountry MarketCountry { get; }
	}
}