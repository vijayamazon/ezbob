namespace FreeAgent
{
	using EZBob.DatabaseLib.Model;
	using StructureMap;

	public class FreeAgentConfig
    {
		public FreeAgentConfig()
		{
			var configurationVariablesRepository = ObjectFactory.GetInstance<ConfigurationVariablesRepository>();

			OAuthIdentifier = configurationVariablesRepository.GetByName("FreeAgentOAuthIdentifier");
			OAuthSecret = configurationVariablesRepository.GetByName("FreeAgentOAuthSecret");
			OAuthAuthorizationEndpoint = configurationVariablesRepository.GetByName("FreeAgentOAuthAuthorizationEndpoint");
			OAuthTokenEndpoint = configurationVariablesRepository.GetByName("FreeAgentOAuthTokenEndpoint");
			InvoicesRequest = configurationVariablesRepository.GetByName("FreeAgentInvoicesRequest");
			InvoicesRequestMonthPart = configurationVariablesRepository.GetByName("FreeAgentInvoicesRequestMonthPart");
			CompanyRequest = configurationVariablesRepository.GetByName("FreeAgentCompanyRequest");
			UsersRequest = configurationVariablesRepository.GetByName("FreeAgentUsersRequest");
			ExpensesRequest = configurationVariablesRepository.GetByName("FreeAgentExpensesRequest");
			ExpensesRequestDatePart = configurationVariablesRepository.GetByName("FreeAgentExpensesRequestDatePart");
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
