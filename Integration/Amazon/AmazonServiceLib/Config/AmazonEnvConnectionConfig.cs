using EzBob.AmazonServiceLib.Common;
using Scorto.Configuration;

namespace EzBob.AmazonServiceLib.Config
{
	public class AmazonEnvConnectionConfig : ConfigurationRoot, IAmazonMarketPlaceTypeConnection
    {
        public string KeyId
        {
            get { return GetValue<string>("KeyId"); }
        }

        public string SecretKeyId
        {
            get { return GetValue<string>("SecretKeyId"); }
        }

        public string AskvilleAmazonLogin
        {
            get { return GetValue<string>("AskvilleAmazonLogin"); }
        }

        public string AskvilleAmazonPass
        {
            get { return GetValue<string>("AskvilleAmazonPass").Decrypt(); }
        }

        public AmazonServiceType ServiceType
        {
            get { return GetValue<AmazonServiceType>("ServiceType"); }
        }

        public AmazonServiceCountry MarketCountry
        {
            get { return GetValue<AmazonServiceCountry>("MarketCountry"); }
        }
    }
}