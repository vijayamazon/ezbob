using Scorto.Configuration;

namespace EzBob.Configuration
{
    public class PayPointConfiguration : ConfigurationRoot
    {
        public string ServiceUrl
        {
            get { return GetValue<string>("ServiceUrl"); }
        }

        public string Mid
        {
            get { return GetValue<string>("Mid"); }
        }

        public string VpnPassword
        {
            get { return GetValue<string>("VpnPassword").Decrypt(); }
        }

        public string RemotePassword
        {
            get { return GetValue<string>("RemotePassword").Decrypt(); }
        }

        public string TemplateUrl
        {
            get
            {
                return GetValueWithDefault<string>("TemplateUrl",
                                                   "http://www.secpay.com/users/secpay/ezbob-template.html");
            }
        }

        public string Options
        {
            get { return GetValueWithDefault<string>("Options", "test_status=true"); }
        }

        public bool ValidateName
        {
            get { return GetValueWithDefault<bool>("ValidateName", "False"); }
        }

        public bool DebugMode
        {
            get { return GetValueWithDefault<bool>("DebugMode", "False"); }
        }

        public bool IsValidCard
        {
            get { return GetValueWithDefault<bool>("IsValidCard", "True"); }
        }

        public bool EnableCardLimit
        {
            get { return GetValueWithDefault<bool>("EnableCardLimit", "False"); }
        }

        public decimal CardLimitAmount
        {
            get { return GetValueWithDefault<decimal>("CardLimitAmount", "500"); }
        }

        public bool EnableDebugErrorCodeN
        {
            get { return GetValueWithDefault<bool>("EnableDebugErrorCodeN", "true"); }
        }
    }
}