namespace Sage.Config
{
	public interface ISageConfig
	{
		string OAuthIdentifier { get; }
		string OAuthSecret { get; }
		string OAuthAuthorizationEndpoint { get; }
		string OAuthTokenEndpoint { get; }
		string SalesInvoicesRequest { get; }
		string PurchaseInvoicesRequest { get; }
		string IncomesRequest { get; }
		string ExpendituresRequest { get; }
		string RequestForDatesPart { get; }
    }
}