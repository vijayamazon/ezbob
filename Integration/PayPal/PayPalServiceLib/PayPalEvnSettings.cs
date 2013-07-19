using EzBob.CommonLib;
using Scorto.Configuration;

namespace EzBob.PayPalServiceLib
{
	public class ErrorRetryingInfoEnvSettings : ConfigurationRoot
	{
		private bool UseLastTimeOut
		{
			get { return GetValueWithDefault<bool>( "UseLastTimeOut", "False" ); }
		}

		private int MinorTimeOutInSeconds
		{
			get { return GetValueWithDefault<int>( "MinorTimeOutInSeconds", "60" ); }

		}

		private bool EnableRetrying
		{
			get { return GetValueWithDefault<bool>( "EnableRetrying", "True" ); }
		}

		public ErrorRetryingInfo ErrorRetryingInfo
		{
			get 
			{
				var data = new ErrorRetryingInfo( EnableRetrying, MinorTimeOutInSeconds, UseLastTimeOut );

				return data;
			}
		}
	}

	public class PayPalEvnSettings : ConfigurationRoot, IPayPalMarketplaceSettings
	{
		public ErrorRetryingInfo ErrorRetryingInfo 
		{
			get
			{
				var xml = GetConfiguration<CustomXmlConfiguration>( "ErrorRetryingInfo" );
				return SerializeDataHelper.DeserializeTypeFromString<ErrorRetryingInfo>( xml.Loader.ConfigurationElement.InnerXml);
			}
		}

		public int MonthsBack
		{
			get { return GetValueWithDefault<int>( "TransactionSerchMonthsBack", "12" ); }
		}

		public int MaxMonthsPerRequest
		{
			get { return GetValueWithDefault<int>( "MaxMonthsPerRequest", "3" ); }
		}

		public int OpenTimeOutInMinutes
		{
			get { return GetValueWithDefault<int>( "OpenTimeOutInMinutes", "1" ); }
		}

		public int SendTimeoutInMinutes
		{
			get { return GetValueWithDefault<int>( "SendTimeoutInMinutes", "1" ); }
		}

	    public bool EnableCategories
	    {
            get { return GetValueWithDefault<bool>("EnableCategories", "false"); }
	    }
	}
}