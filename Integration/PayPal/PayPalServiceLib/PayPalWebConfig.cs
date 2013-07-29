using System;
using System.Configuration;
using System.Globalization;
using EzBob.PayPalServiceLib.Common;

namespace EzBob.PayPalServiceLib
{
    public class PayPalWebConfig : IPayPalConfig
    {
    	public ServiceEndPointType ServiceType
    	{
			get 
			{
				var val = ConfigurationManager.AppSettings["API_SERVICE_TYPE"];
				return (ServiceEndPointType)Enum.Parse( typeof( ServiceEndPointType ), val, true );				
			}
    	}

		public PayPalProfileType ApiAuthenticationMode
        {
            get 
			{
				var val = ConfigurationManager.AppSettings["API_AUTHENTICATION_MODE"];
				return (PayPalProfileType)Enum.Parse( typeof( PayPalProfileType ), val, true );
			}
        }

        public string PPApplicationId
        {
            get { return ConfigurationManager.AppSettings["APPLICATION-ID"]; }
        }

        public string ApiUsername
        {
            get { return ConfigurationManager.AppSettings["API_USERNAME"]; }
        }

        public string ApiPassword
        {
            get { return ConfigurationManager.AppSettings["API_PASSWORD"]; }
        }

        public string ApiSignature
        {
            get { return ConfigurationManager.AppSettings["API_SIGNATURE"]; }
        }

        public PayPalDataFormat ApiRequestformat
        {
			get
			{
				var val = ConfigurationManager.AppSettings["API_REQUESTFORMAT"];
				return (PayPalDataFormat)Enum.Parse( typeof( PayPalDataFormat ), val, true );
			}
        }

		public PayPalDataFormat ApiResponseformat
        {
            get 
			{
				var val = ConfigurationManager.AppSettings["API_RESPONSEFORMAT"];
				return (PayPalDataFormat)Enum.Parse( typeof( PayPalDataFormat ), val, true );				
			}
        }

		public bool TrustAll
		{
			get { return Convert.ToBoolean(ConfigurationManager.AppSettings["TrustAll"], CultureInfo.InvariantCulture); }
		}

		public int NumberOfRetries
		{
			get { return int.Parse(ConfigurationManager.AppSettings["NumberOfRetries"], CultureInfo.InvariantCulture); }
		}
    }
}