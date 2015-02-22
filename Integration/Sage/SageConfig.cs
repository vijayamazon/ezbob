namespace Sage {
	using System;
	using ConfigManager;

	public class SageConfig {
		public SageConfig() {
			OAuthIdentifier = CurrentValues.Instance.SageOAuthIdentifier.Value;
			OAuthSecret = CurrentValues.Instance.SageOAuthSecret.Value;
			SalesInvoicesRequest = CurrentValues.Instance.SageSalesInvoicesRequest.Value;
			PurchaseInvoicesRequest = CurrentValues.Instance.SagePurchaseInvoicesRequest.Value;
			IncomesRequest = CurrentValues.Instance.SageIncomesRequest.Value;
			ExpendituresRequest = CurrentValues.Instance.SageExpendituresRequest.Value;
			PaymentStatusesRequest = CurrentValues.Instance.SagePaymentStatusesRequest.Value;
			RequestForDatesPart = CurrentValues.Instance.SageRequestForDatesPart.Value;

			var uri = new Uri(CurrentValues.Instance.SageOAuthAuthorizationEndpoint.Value);
			OAuthApprovalRequestPath = uri.AbsolutePath;
			OAuthApprovalRequestHost = string.Format(
				"{0}://{1}{2}",
				uri.Scheme,
				uri.Host,
				uri.IsDefaultPort ? string.Empty : ":" + uri.Port
			);

			uri = new Uri(CurrentValues.Instance.SageOAuthTokenEndpoint.Value);
			OAuthTokenRequestPath = uri.AbsolutePath;
			OAuthTokenRequestHost = string.Format(
				"{0}://{1}{2}",
				uri.Scheme,
				uri.Host,
				uri.IsDefaultPort ? string.Empty : ":" + uri.Port
			);
		} // SageConfig

		public const string SageBaseUri = "https://app.sageone.com";
		
		public string OAuthIdentifier { get; set; }
		public string OAuthSecret { get; set; }

		public string OAuthApprovalRequestPath { get; set; }
		public string OAuthApprovalRequestHost { get; set; }

		public string OAuthTokenRequestPath { get; set; }
		public string OAuthTokenRequestHost { get; set; }

		public string SalesInvoicesRequest { get; set; }
		public string PurchaseInvoicesRequest { get; set; }
		public string IncomesRequest { get; set; }
		public string ExpendituresRequest { get; set; }
		public string PaymentStatusesRequest { get; set; }
		public string RequestForDatesPart { get; set; }
    }
}
