using Scorto.Configuration;

namespace EzBob.Configuration
{
    public class ExperianIntegrationParams : ConfigurationRoot
    {
        public string CertificateThumb
        {
            get { return GetValue<string>("CertificateThumb"); }
        }

        public string UIdCertificateThumb
        {
            get { return GetValue<string>("UIdCertificateThumb"); }
        }

        public string InteractiveService
        {
            get { return GetValue<string>("InteractiveService"); }
        }

        public string IdHubService
        {
            get { return GetValue<string>("IdHubService"); }
        }

        public string AuthTokenServiceIdHub
        {
            get { return GetValue<string>("AuthTokenServiceIdHub"); }
        }

        public string AuthTokenService
        {
            get { return GetValue<string>("AuthTokenService"); }
        }

        public string ESeriesUrl
        {
            get { return GetValue<string>("ESeriesUrl"); }
        }

        public int UpdateBusinessDataPeriodDays
        {
            get { return GetValue<int>("UpdateBusinessDataPeriodDays"); }
        }

        public string SecureFtpHostName
        {
            get { return GetValue<string>("SecureFtpHostName"); }
        }

        public string SecureFtpUserName
        {
            get { return GetValue<string>("SecureFtpUserName"); }
        }

        public string SecureFtpUserPassword
        {
            get { return GetValue<string>("SecureFtpUserPassword").Decrypt(); }
        }

        public string InteractiveMode
        {
            get { return GetValue<string>("InteractiveMode"); }
        }
    }
}