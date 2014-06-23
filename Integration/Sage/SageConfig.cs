namespace Sage {
	using ConfigManager;

	public class SageConfig {
		public SageConfig() {
			OAuthIdentifier = CurrentValues.Instance.SageOAuthIdentifier;
			OAuthSecret = CurrentValues.Instance.SageOAuthSecret;
			OAuthAuthorizationEndpoint = CurrentValues.Instance.SageOAuthAuthorizationEndpoint;
			OAuthTokenEndpoint = CurrentValues.Instance.SageOAuthTokenEndpoint;
			SalesInvoicesRequest = CurrentValues.Instance.SageSalesInvoicesRequest;
			PurchaseInvoicesRequest = CurrentValues.Instance.SagePurchaseInvoicesRequest;
			IncomesRequest = CurrentValues.Instance.SageIncomesRequest;
			ExpendituresRequest = CurrentValues.Instance.SageExpendituresRequest;
			PaymentStatusesRequest = CurrentValues.Instance.SagePaymentStatusesRequest;
			RequestForDatesPart = CurrentValues.Instance.SageRequestForDatesPart;
		}

		public string OAuthIdentifier { get; set; }
		public string OAuthSecret { get; set; }
		public string OAuthAuthorizationEndpoint { get; set; }
		public string OAuthTokenEndpoint { get; set; }
		public string SalesInvoicesRequest { get; set; }
		public string PurchaseInvoicesRequest { get; set; }
		public string IncomesRequest { get; set; }
		public string ExpendituresRequest { get; set; }
		public string PaymentStatusesRequest { get; set; }
		public string RequestForDatesPart { get; set; }
    }
}
