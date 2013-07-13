namespace Sage.Config
{
	public interface ISageConfig
	{
		string OAuthIdentifier { get; }
		string OAuthSecret { get; }
		string OAuthAuthorizationEndpoint { get; }
		string OAuthTokenEndpoint { get; }
		string InvoicesRequest { get; }
		string InvoicesRequestMonthPart { get; }
    }
}