using EzBob.PayPalServiceLib.Common;
using Scorto.Configuration;

namespace EzBob.PayPalServiceLib
{
	public class PayPalEnvConfig : ConfigurationRoot, IPayPalConfig
    {
    	public ServiceEndPointType ServiceType
    	{
			get { return GetValue<ServiceEndPointType>( "ServiceType" ); }
    	}

		public PayPalProfileType ApiAuthenticationMode
        {
			get { return GetValue<PayPalProfileType>( "ApiAuthenticationMode" ); }
        }

        public string PPApplicationId
        {
            get { return GetValue<string>("PPApplicationId"); }
        }

        public string ApiUsername
        {
            get { return GetValue<string>("ApiUsername"); }
        }

        public string ApiPassword
        {
            get { return GetValue<string>("ApiPassword").Decrypt(); }
        }

        public string ApiSignature
        {
            get { return GetValue<string>("ApiSignature"); }
        }

        public PayPalDataFormat ApiRequestformat
        {
			get { return GetValue<PayPalDataFormat>( "ApiRequestformat" ); }
        }

		public PayPalDataFormat ApiResponseformat
        {
			get { return GetValue<PayPalDataFormat>( "ApiResponseformat" ); }
        }

        public bool TrustAll
        {
            get { return GetValue<bool>("TrustAll"); }
        }	
    }
}