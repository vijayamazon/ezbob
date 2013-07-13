namespace Sage.Config
{
	using Scorto.Configuration;

	public class SageEnvConnectionConfig : ConfigurationRoot, ISageConfig
    {
		public string OAuthIdentifier
        {
            get { return GetValue<string>("OAuthIdentifier"); }
        }

        public string OAuthSecret
        {
            get { return GetValue<string>("OAuthSecret"); }
        }

        public string OAuthAuthorizationEndpoint
        {
            get { return GetValue<string>("OAuthAuthorizationEndpoint"); }
        }

        public string OAuthTokenEndpoint
        {
			get { return GetValue<string>("OAuthTokenEndpoint"); }
        }

        public string InvoicesRequest
        {
            get { return GetValue<string>("InvoicesRequest"); }
        }

        public string InvoicesRequestMonthPart
        {
            get { return GetValue<string>("InvoicesRequestMonthPart"); }
        }
    }
}