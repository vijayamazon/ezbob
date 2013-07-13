namespace Sage.Config
{
	public class SageConfigRoot : ISageConfig
    {
		public string OAuthIdentifier { get; set; }
		public string OAuthSecret { get; set; }
		public string OAuthAuthorizationEndpoint { get; set; }
		public string OAuthTokenEndpoint { get; set; }
		public string InvoicesRequest { get; set; }
		public string InvoicesRequestMonthPart { get; set; }
    }
}
