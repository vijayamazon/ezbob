namespace FreeAgent.Config
{
	public interface IFreeAgentConfig
	{
		string OAuthIdentifier { get; }
		string OAuthSecret { get; }
		string OAuthAuthorizationEndpoint { get; }
		string OAuthTokenEndpoint { get; }
		string InvoicesRequest { get; }
		string InvoicesRequestMonthPart { get; }
		string CompanyRequest { get; }
		string UsersRequest { get; }
		string ExpensesRequest { get; }
		string ExpensesRequestDatePart { get; }
    }
}