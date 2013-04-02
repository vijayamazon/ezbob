using EzBob.CommonLib;
using Scorto.Configuration;

namespace EzBob.AmazonServiceLib.Config
{
	public class AmazonEvnSettings : ConfigurationRoot, IAmazonMarketplaceSettings
	{
		public ErrorRetryingInfo ErrorRetryingInfo
		{
			get
			{
				var xml = GetConfiguration<CustomXmlConfiguration>( "ErrorRetryingInfo" );
				return SerializeDataHelper.DeserializeTypeFromString<ErrorRetryingInfo>( xml.Loader.ConfigurationElement.InnerXml );
			}
		}

	}
}