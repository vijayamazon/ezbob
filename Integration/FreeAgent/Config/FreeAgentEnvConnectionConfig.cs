namespace FreeAgent.Config
{
	using Scorto.Configuration;

	public class FreeAgentEnvConnectionConfig : ConfigurationRoot, IFreeAgentConfig
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

        public string CompanyRequest
        {
            get { return GetValue<string>("CompanyRequest"); }
        }

        public string UsersRequest
        {
            get { return GetValue<string>("UsersRequest"); }
        }

        public string ExpensesRequest
        {
            get { return GetValue<string>("ExpensesRequest"); }
        }

        public string ExpensesRequestDatePart
        {
            get { return GetValue<string>("ExpensesRequestDatePart"); }
        }
    }
}