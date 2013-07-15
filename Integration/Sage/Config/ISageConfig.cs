namespace Sage.Config
{
	public interface ISageConfig
	{
		string OAuthIdentifier { get; }
		string OAuthSecret { get; }
		string OAuthAuthorizationEndpoint { get; }
		string OAuthTokenEndpoint { get; }
		string SalesInvoicesRequest { get; }
		string RequestForDatesPart { get; }
		string IncomesRequest { get; }
		string PurchaseInvoicesRequest { get; }
    }
}