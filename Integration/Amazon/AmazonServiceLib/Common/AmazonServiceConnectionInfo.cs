using EzBob.AmazonServiceLib.Config;

namespace EzBob.AmazonServiceLib.Common
{
	public class AmazonServiceConnectionInfo
	{
		public AmazonServiceCountry MarketCountry { get; set; }
		public AmazonApplicationInfo ApplicationInfo { get; set; }
		public AmazonDeveloperAccessInfo AccessInfo { get; set; }
		public AmazonServiceType ServiceType { get; set; }
	}

	public static class AmazonServiceConnectionFactory
	{
		public static AmazonServiceConnectionInfo CreateConnection( IAmazonMarketPlaceTypeConnection connectionInfo )
		{
			return new AmazonServiceConnectionInfo
			{
			    AccessInfo = new AmazonDeveloperAccessInfo
			    {
			       	KeyId = connectionInfo.KeyId,
			       	SecretKeyId = connectionInfo.SecretKeyId
			    },

				ApplicationInfo = new AmazonApplicationInfo
			    {
			       	Name = "C#",
			       	Version = "4.0"
			    },

			    MarketCountry = connectionInfo.MarketCountry,
			    ServiceType = connectionInfo.ServiceType
			};
		}
	}
}