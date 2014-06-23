namespace FreeAgent {
	using ConfigManager;

	public class FreeAgentConfig {
		public FreeAgentConfig() {
			OAuthIdentifier = CurrentValues.Instance.FreeAgentOAuthIdentifier;
			OAuthSecret = CurrentValues.Instance.FreeAgentOAuthSecret;
			OAuthAuthorizationEndpoint = CurrentValues.Instance.FreeAgentOAuthAuthorizationEndpoint;
			OAuthTokenEndpoint = CurrentValues.Instance.FreeAgentOAuthTokenEndpoint;
			InvoicesRequest = CurrentValues.Instance.FreeAgentInvoicesRequest;
			InvoicesRequestMonthPart = CurrentValues.Instance.FreeAgentInvoicesRequestMonthPart;
			CompanyRequest = CurrentValues.Instance.FreeAgentCompanyRequest;
			UsersRequest = CurrentValues.Instance.FreeAgentUsersRequest;
			ExpensesRequest = CurrentValues.Instance.FreeAgentExpensesRequest;
			ExpensesRequestDatePart = CurrentValues.Instance.FreeAgentExpensesRequestDatePart;
		}

		public string OAuthIdentifier { get; set; }
		public string OAuthSecret { get; set; }
		public string OAuthAuthorizationEndpoint { get; set; }
		public string OAuthTokenEndpoint { get; set; }
		public string InvoicesRequest { get; set; }
		public string InvoicesRequestMonthPart { get; set; }
		public string CompanyRequest { get; set; }
		public string UsersRequest { get; set; }
		public string ExpensesRequest { get; set; }
		public string ExpensesRequestDatePart { get; set; }
	}
}
