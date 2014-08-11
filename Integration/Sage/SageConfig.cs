namespace Sage {
	using ConfigManager;

	public class SageConfig {
		public SageConfig() {
			OAuthIdentifier = CurrentValues.Instance.SageOAuthIdentifier.Value;
			OAuthSecret = CurrentValues.Instance.SageOAuthSecret.Value;
			OAuthAuthorizationEndpoint = CurrentValues.Instance.SageOAuthAuthorizationEndpoint.Value;
			OAuthTokenEndpoint = CurrentValues.Instance.SageOAuthTokenEndpoint.Value;
			SalesInvoicesRequest = CurrentValues.Instance.SageSalesInvoicesRequest.Value;
			PurchaseInvoicesRequest = CurrentValues.Instance.SagePurchaseInvoicesRequest.Value;
			IncomesRequest = CurrentValues.Instance.SageIncomesRequest.Value;
			ExpendituresRequest = CurrentValues.Instance.SageExpendituresRequest.Value;
			PaymentStatusesRequest = CurrentValues.Instance.SagePaymentStatusesRequest.Value;
			RequestForDatesPart = CurrentValues.Instance.SageRequestForDatesPart.Value;
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
