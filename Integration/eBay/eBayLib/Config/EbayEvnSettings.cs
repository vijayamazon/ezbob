using EzBob.CommonLib;
using Scorto.Configuration;

namespace EzBob.eBayLib.Config
{
	public class EbayEvnSettings : ConfigurationRoot, IEbayMarketplaceSettings
	{
		//private bool _OrdersFromTeraPeakOnly;

		public bool DownloadCategories
		{
			get { return GetValueWithDefault<bool>( "DownloadCategories", "False" ); }
		}

		public ErrorRetryingInfo ErrorRetryingInfo
		{
			get 
			{
				var xml = GetConfiguration<CustomXmlConfiguration>( "ErrorRetryingInfo" );
				return SerializeDataHelper.DeserializeTypeFromString<ErrorRetryingInfo>( xml.Loader.ConfigurationElement.InnerXml );
			}
		}

		public bool OrdersFromTeraPeakOnly
		{
			get { return GetValueWithDefault<bool>( "OrdersFromTeraPeakOnly", "False" ); }
			
		}

		public bool DisableUpdate
		{
			get { return GetValueWithDefault<bool>( "DisableUpdate", "False" ); }
		}
	}
}