namespace Sage
{
    using System;
    using System.Collections.Generic;
    using System.Web.Script.Serialization;
    using Config;
    using EZBob.DatabaseLib.DatabaseWrapper.Order;
    using RestSharp;
	using log4net;
	using StructureMap;

	public class SageConnector
	{
		private static readonly ISageConfig config = ObjectFactory.GetInstance<ISageConfig>();
		private static readonly ILog log = LogManager.GetLogger(typeof(SageConnector));

		public static SagePurchaseInvoicesList GetPurchaseInvoices(string accessToken, DateTime? fromDate)
		{
			List<SagePurchaseInvoice> purchaseInvoices = ExecuteRequestAndGetDeserializedResponse<SagePurchaseInvoicesListHelper, List<SagePurchaseInvoice>>(accessToken, config.PurchaseInvoicesRequest, fromDate, CreateDeserializedPurchaseInvoices, FillPurchaseInvoicesFromDeserializedData);
			var purchaseInvoicesList = new SagePurchaseInvoicesList(DateTime.UtcNow, purchaseInvoices);
			return purchaseInvoicesList;
		}

		private static SagePurchaseInvoicesListHelper CreateDeserializedPurchaseInvoices(string cleanResponse)
		{
			var deserializedPurchaseInvoices = DeserializePurchaseInvoices(cleanResponse);
			if (deserializedPurchaseInvoices == null)
			{
				log.Error("Error deserializing sage purchase invoices");
				return null;
			}
			if (deserializedPurchaseInvoices.diagnoses != null)
			{
				foreach (SageDiagnostic diagnostic in deserializedPurchaseInvoices.diagnoses)
				{
					log.ErrorFormat("Error occured during sage purchase invoices request. Purchase invoices were not fetched. Message:{0} Source:{1} Severity:{2} DataCode:{3}",
						diagnostic.message, diagnostic.source, diagnostic.severity, diagnostic.dataCode);
				}
				return null;
			}

			return deserializedPurchaseInvoices;
		}

		private static SagePurchaseInvoicesListHelper DeserializePurchaseInvoices(string cleanResponse)
		{
			try
			{
				var js = new JavaScriptSerializer();
				return ((SagePurchaseInvoicesListHelper)js.Deserialize(cleanResponse, typeof(SagePurchaseInvoicesListHelper)));
			}
			catch (Exception e)
			{
				log.ErrorFormat("Failed deserializing sage purchase invoices response:{0}. The error was:{1}", cleanResponse, e);
				return null;
			}
		}

		private static bool FillPurchaseInvoicesFromDeserializedData(SagePurchaseInvoicesListHelper deserializePurchaseInvoicesResponse, List<SagePurchaseInvoice> purchaseInvoices)
		{
			foreach (var serializedPurchaseInvoice in deserializePurchaseInvoicesResponse.resources)
			{
				TryDeserializePurchaseInvoice(purchaseInvoices, serializedPurchaseInvoice);
			}

			return true;
		}

		private static void TryDeserializePurchaseInvoice(List<SagePurchaseInvoice> purchaseInvoices, SagePurchaseInvoiceSerialization serializaedPurchaseInvoice)
		{
			try
			{
				purchaseInvoices.Add(SageDesreializer.DeserializePurchaseInvoice(serializaedPurchaseInvoice));
			}
			catch (Exception)
			{
				log.ErrorFormat("Failed creating purchase invoice for SageId:{0}. Purchase invoice won't be handled!", serializaedPurchaseInvoice.id);
			}
		}

		public static SageIncomesList GetIncomes(string accessToken, DateTime? fromDate)
		{
			List<SageIncome> incomes = ExecuteRequestAndGetDeserializedResponse<SageIncomesListHelper, List<SageIncome>>(accessToken, config.IncomesRequest, fromDate, CreateDeserializedIncomes, FillIncomesFromDeserializedData);
			var incomesList = new SageIncomesList(DateTime.UtcNow, incomes);
			return incomesList;
		}

		private static SageIncomesListHelper CreateDeserializedIncomes(string cleanResponse)
		{
			var deserializeIncomes = DeserializeIncomes(cleanResponse);
			if (deserializeIncomes == null)
			{
				log.Error("Error deserializing sage incomes");
				return null;
			}
			if (deserializeIncomes.diagnoses != null)
			{
				foreach (SageDiagnostic diagnostic in deserializeIncomes.diagnoses)
				{
					log.ErrorFormat("Error occured during sage incomes request. Incomes were not fetched. Message:{0} Source:{1} Severity:{2} DataCode:{3}",
						diagnostic.message, diagnostic.source, diagnostic.severity, diagnostic.dataCode);
				}
				return null;
			}

			return deserializeIncomes;
		}

		private static SageIncomesListHelper DeserializeIncomes(string cleanResponse)
		{
			try
			{
				var js = new JavaScriptSerializer();
				return ((SageIncomesListHelper)js.Deserialize(cleanResponse, typeof(SageIncomesListHelper)));
			}
			catch (Exception e)
			{
				log.ErrorFormat("Failed deserializing sage incomes response:{0}. The error was:{1}", cleanResponse, e);
				return null;
			}
		}

		private static bool FillIncomesFromDeserializedData(SageIncomesListHelper deserializeIncomesResponse, List<SageIncome> incomes)
		{
			foreach (var serializedIncome in deserializeIncomesResponse.resources)
			{
				TryDeserializeIncome(incomes, serializedIncome);
			}

			return true;
		}

		private static void TryDeserializeIncome(List<SageIncome> incomes, SageIncomeSerialization serializedIncome)
		{
			try
			{
				incomes.Add(SageDesreializer.DeserializeIncome(serializedIncome));
			}
			catch (Exception)
			{
				log.ErrorFormat("Failed creating income for SageId:{0}. Income won't be handled!", serializedIncome.id);
			}
		}

		public static SageSalesInvoicesList GetSalesInvoices(string accessToken, DateTime? fromDate)
		{
			List<SageSalesInvoice> salesInvoices = ExecuteRequestAndGetDeserializedResponse<SageSalesInvoicesListHelper, List<SageSalesInvoice>>(accessToken, config.SalesInvoicesRequest, fromDate, CreateDeserializedSalesInvoices, FillSalesInvoicesFromDeserializedData);
			var salesInvoicesList = new SageSalesInvoicesList(DateTime.UtcNow, salesInvoices);
			return salesInvoicesList;
		}

		private static SageSalesInvoicesListHelper CreateDeserializedSalesInvoices(string cleanResponse)
		{
			var deserializedSalesInvoices = DeserializeSalesInvoices(cleanResponse);
			if (deserializedSalesInvoices == null)
			{
				log.Error("Error deserializing sage sales invoices");
				return null;
			}
			if (deserializedSalesInvoices.diagnoses != null)
			{
				foreach (SageDiagnostic diagnostic in deserializedSalesInvoices.diagnoses)
				{
					log.ErrorFormat("Error occured during sage sales invoices request. Sales invoices were not fetched. Message:{0} Source:{1} Severity:{2} DataCode:{3}",
						diagnostic.message, diagnostic.source, diagnostic.severity, diagnostic.dataCode);
				}
				return null;
			}

			return deserializedSalesInvoices;
		}

		private static SageSalesInvoicesListHelper DeserializeSalesInvoices(string cleanResponse)
		{
			try
			{
				var js = new JavaScriptSerializer();
				return ((SageSalesInvoicesListHelper)js.Deserialize(cleanResponse, typeof(SageSalesInvoicesListHelper)));
			}
			catch (Exception e)
			{
				log.ErrorFormat("Failed deserializing sage sales invoices response:{0}. The error was:{1}", cleanResponse, e);
				return null;
			}
		}

		private static bool FillSalesInvoicesFromDeserializedData(SageSalesInvoicesListHelper deserializeSalesInvoicesResponse, List<SageSalesInvoice> salesInvoices)
		{
			foreach (var serializedSalesInvoice in deserializeSalesInvoicesResponse.resources)
			{
				TryDeserializeSalesInvoice(salesInvoices, serializedSalesInvoice);
			}

			return true;
		}

		private static void TryDeserializeSalesInvoice(List<SageSalesInvoice> salesInvoices, SageSalesInvoiceSerialization serializaedSalesInvoice)
		{
			try
			{
				salesInvoices.Add(SageDesreializer.DeserializeSalesInvoice(serializaedSalesInvoice));
			}
			catch (Exception)
			{
				log.ErrorFormat("Failed creating sales invoice for SageId:{0}. Sales invoice won't be handled!", serializaedSalesInvoice.id);
			}
		}
		
		private static TFinalOutput ExecuteRequestAndGetDeserializedResponse<TDeserialize, TFinalOutput>(string accessToken, string requestUrl, DateTime? fromDate, Func<string, TDeserialize> deserializeFunction, Func<TDeserialize, TFinalOutput, bool> generateOutput)
			where TDeserialize : PaginatedResultsBase
			where TFinalOutput : class, new()
		{
			string authorizationHeader = string.Format("Bearer {0}", accessToken);
			string monthPart = !fromDate.HasValue ? string.Empty : string.Format("?{0}", string.Format(config.RequestForDatesPart, fromDate.Value.ToString("dd/MM/yyyy"), DateTime.UtcNow.ToString("dd/MM/yyyy")).Replace('-', '/'));
			string fullRequest = string.Format("{0}{1}", requestUrl, monthPart);
			var request = new RestRequest(Method.GET) { Resource = fullRequest };
			request.AddHeader("Authorization", authorizationHeader);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			TDeserialize deserializedResponse = deserializeFunction(CleanResponse(response.Content));
			if (deserializedResponse == null)
			{
				log.Error("Sage response deserialization failed");
				return new TFinalOutput();
			}

			var results = new TFinalOutput();
			generateOutput(deserializedResponse, results);

			string nextUrl = GetNextUrl(deserializedResponse, fullRequest);

			while (nextUrl != null)
			{
				request = new RestRequest(Method.GET) { Resource = nextUrl };
				request.AddHeader("Authorization", authorizationHeader);
				response = client.Execute(request);

				deserializedResponse = deserializeFunction(CleanResponse(response.Content));
				if (deserializedResponse == null)
				{
					log.Error("Sage response deserialization failed");
					return new TFinalOutput();
				}

				generateOutput(deserializedResponse, results);

				nextUrl = GetNextUrl(deserializedResponse, fullRequest);
			}

			return results;
		}

		private static string CleanResponse(string originalResponse)
		{
			return originalResponse.Replace("\"$", "\"");
		}

		private static string GetNextUrl(PaginatedResultsBase pagenatedResults, string request)
		{
			int nextIndex = pagenatedResults.startIndex + pagenatedResults.itemsPerPage;
			if (pagenatedResults.totalResults <= nextIndex)
			{
				return null;
			}

			return string.Format("{0}{1}$startIndex={2}", request, request.Contains("?") ? "&" : "?", nextIndex);
		}

		public static AccessTokenContainer GetToken(string code, string redirectVal, out string errorMessage)
		{
			string accessTokenRequest = string.Format("{0}?grant_type=authorization_code&code={1}&redirect_uri={2}&scope=&client_secret={3}&client_id={4}",
													  config.OAuthTokenEndpoint, code, redirectVal, config.OAuthSecret, config.OAuthIdentifier);
			
			var request = new RestRequest(Method.POST) { Resource = accessTokenRequest };
			request.AddHeader("Accept", "application / json");
			var client = new RestClient();
			errorMessage = null;
			try
			{
				IRestResponse response = client.Execute(request);
				if (response != null)
				{
					var js = new JavaScriptSerializer();
					var accessTokenContainer = (AccessTokenContainer)js.Deserialize(response.Content, typeof(AccessTokenContainer));
					return accessTokenContainer;
				}
			}
			catch (Exception e)
			{
				errorMessage = "Failure getting access token";
				log.WarnFormat("{0}. Exception:{1}", errorMessage, e);
			}
			return null;
		}

		public static string GetApprovalRequest(string redirectUri)
		{
			return string.Format("{0}?redirect_uri={1}&response_type=code&client_id={2}",
			                     config.OAuthAuthorizationEndpoint, redirectUri, config.OAuthIdentifier);
		}
	}
}