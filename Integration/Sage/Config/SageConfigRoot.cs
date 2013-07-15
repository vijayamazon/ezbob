namespace Sage.Config
{
	public class SageConfigRoot : ISageConfig
    {
		public string OAuthIdentifier { get; set; }
		public string OAuthSecret { get; set; }
		public string OAuthAuthorizationEndpoint { get; set; }
		public string OAuthTokenEndpoint { get; set; }
		public string SalesInvoicesRequest { get; set; }
		public string RequestForDatesPart { get; set; }
		public string IncomesRequest { get; set; }
		public string PurchaseInvoicesRequest { get; set; }
    }
}
