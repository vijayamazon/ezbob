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

		public string SalesInvoicesRequest
        {
			get { return GetValue<string>("SalesInvoicesRequest"); }
        }

		public string PurchaseInvoicesRequest
		{
			get { return GetValue<string>("PurchaseInvoicesRequest"); }
		}

		public string IncomesRequest
		{
			get { return GetValue<string>("IncomesRequest"); }
		}

		public string ExpendituresRequest
		{
			get { return GetValue<string>("ExpendituresRequest"); }
		}

		public string RequestForDatesPart
		{
			get { return GetValue<string>("RequestForDatesPart"); }
		}
    }
}