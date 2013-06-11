namespace FreeAgent.Config
{
	public class FreeAgentConfigRoot : IFreeAgentConfig
    {
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
