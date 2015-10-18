namespace FreeAgent {
	using ConfigManager;

	public class FreeAgentConfig {
		public FreeAgentConfig() {
			ApiBase = CurrentValues.Instance.FreeAgentApiBase;

			OAuthIdentifier = CurrentValues.Instance.FreeAgentOAuthIdentifier;
			OAuthSecret = CurrentValues.Instance.FreeAgentOAuthSecret;
			OAuthAuthorizationEndpoint = CurrentValues.Instance.FreeAgentOAuthAuthorizationEndpoint;
			OAuthTokenEndpoint = CurrentValues.Instance.FreeAgentOAuthTokenEndpoint;

			InvoicesRequest = CurrentValues.Instance.FreeAgentInvoicesRequest;
			InvoicesRequestNestedPart = CurrentValues.Instance.FreeAgentInvoicesRequestNestedPart;
			InvoicesRequestMonthPart = CurrentValues.Instance.FreeAgentInvoicesRequestMonthPart;

			CompanyRequest = CurrentValues.Instance.FreeAgentCompanyRequest;

			UsersRequest = CurrentValues.Instance.FreeAgentUsersRequest;

			ExpensesRequest = CurrentValues.Instance.FreeAgentExpensesRequest;
			ExpensesCategoriesRequest = CurrentValues.Instance.FreeAgentExpensesCategoriesRequest;
			ExpensesRequestDatePart = CurrentValues.Instance.FreeAgentExpensesRequestDatePart;
		} // constructor

		public string ApiBase { get; private set; }

		public string OAuthIdentifier { get; private set; }
		public string OAuthSecret { get; private set; }
		public string OAuthAuthorizationEndpoint { get; private set; }
		public string OAuthTokenEndpoint { get; private set; }

		public string InvoicesRequest { get; private set; }
		public string InvoicesRequestNestedPart { get; private set; }
		public string InvoicesRequestMonthPart { get; private set; }

		public string CompanyRequest { get; private set; }

		public string UsersRequest { get; private set; }

		public string ExpensesRequest { get; private set; }
		public string ExpensesCategoriesRequest { get; private set; }
		public string ExpensesRequestDatePart { get; private set; }
	} // class FreeAgentConfig
} // namespace
