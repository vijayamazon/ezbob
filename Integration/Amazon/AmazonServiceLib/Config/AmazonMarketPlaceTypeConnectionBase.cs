using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.Config
{
	public abstract class AmazonMarketPlaceTypeConnectionBase : IAmazonMarketPlaceTypeConnection
	{
		public string KeyId { get; set; }
		public string SecretKeyId { get; set; }
        public string AskvilleAmazonLogin { get; set; }
        public string AskvilleAmazonPass { get; set; }
	    public AmazonServiceType ServiceType { get; set; }
		public AmazonServiceCountry MarketCountry { get; set; }
	}
}