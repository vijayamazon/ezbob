namespace Sage
{
	using EZBob.DatabaseLib.Model;
	using StructureMap;

	public class SageConfig
    {
		public SageConfig()
		{
			var configurationVariablesRepository = ObjectFactory.GetInstance<ConfigurationVariablesRepository>();

			OAuthIdentifier = configurationVariablesRepository.GetByName("SageOAuthIdentifier");
			OAuthSecret = configurationVariablesRepository.GetByName("SageOAuthSecret");
			OAuthAuthorizationEndpoint = configurationVariablesRepository.GetByName("SageOAuthAuthorizationEndpoint");
			OAuthTokenEndpoint = configurationVariablesRepository.GetByName("SageOAuthTokenEndpoint");
			SalesInvoicesRequest = configurationVariablesRepository.GetByName("SageSalesInvoicesRequest");
			PurchaseInvoicesRequest = configurationVariablesRepository.GetByName("SagePurchaseInvoicesRequest");
			IncomesRequest = configurationVariablesRepository.GetByName("SageIncomesRequest");
			ExpendituresRequest = configurationVariablesRepository.GetByName("SageExpendituresRequest");
			PaymentStatusesRequest = configurationVariablesRepository.GetByName("SagePaymentStatusesRequest");
			RequestForDatesPart = configurationVariablesRepository.GetByName("SageRequestForDatesPart");
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
