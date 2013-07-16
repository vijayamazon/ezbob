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

		public static List<SagePaymentStatus> GetPaymentStatuses(string accessToken)
		{
			return ExecuteRequestAndGetDeserializedResponse<SagePaymentStatusDeserialization, SagePaymentStatus>(accessToken, config.PaymentStatusesRequest, null, CreateDeserializedPaymentStatuses, SageDesreializer.DeserializePaymentStatus);
		}

		private static PaginatedResults<SagePaymentStatusDeserialization> CreateDeserializedPaymentStatuses(string cleanResponse)
		{
			var deserializedPaymentStatuses = DeserializePaymentStatuses(cleanResponse);
			if (deserializedPaymentStatuses == null)
			{
				log.Error("Error deserializing sage payment statuses");
				return null;
			}
			if (deserializedPaymentStatuses.diagnoses != null)
			{
				foreach (SageDiagnostic diagnostic in deserializedPaymentStatuses.diagnoses)
				{
					log.ErrorFormat("Error occured during sage payment statuses request. Payment statuses were not fetched. Message:{0} Source:{1} Severity:{2} DataCode:{3}",
						diagnostic.message, diagnostic.source, diagnostic.severity, diagnostic.dataCode);
				}
				return null;
			}

			return deserializedPaymentStatuses;
		}

		private static PaginatedResults<SagePaymentStatusDeserialization> DeserializePaymentStatuses(string cleanResponse)
		{
			try
			{
				var js = new JavaScriptSerializer();
				return ((PaginatedResults<SagePaymentStatusDeserialization>)js.Deserialize(cleanResponse, typeof(PaginatedResults<SagePaymentStatusDeserialization>)));
			}
			catch (Exception e)
			{
				log.ErrorFormat("Failed deserializing sage payment statuses response:{0}. The error was:{1}", cleanResponse, e);
				return null;
			}
		}

		public static SageSalesInvoicesList GetSalesInvoices(string accessToken, DateTime? fromDate)
		{
			List<SageSalesInvoice> salesInvoices = ExecuteRequestAndGetDeserializedResponse<SageSalesInvoiceDeserialization, SageSalesInvoice>(accessToken, config.SalesInvoicesRequest, fromDate, CreateDeserializedSalesInvoices, SageDesreializer.DeserializeSalesInvoice);
			var salesInvoicesList = new SageSalesInvoicesList(DateTime.UtcNow, salesInvoices);
			return salesInvoicesList;
		}

		private static PaginatedResults<SageSalesInvoiceDeserialization> CreateDeserializedSalesInvoices(string cleanResponse)
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

		private static PaginatedResults<SageSalesInvoiceDeserialization> DeserializeSalesInvoices(string cleanResponse)
		{
			try
			{
				var js = new JavaScriptSerializer();
				return ((PaginatedResults<SageSalesInvoiceDeserialization>)js.Deserialize(cleanResponse, typeof(PaginatedResults<SageSalesInvoiceDeserialization>)));
			}
			catch (Exception e)
			{
				log.ErrorFormat("Failed deserializing sage sales invoices response:{0}. The error was:{1}", cleanResponse, e);
				return null;
			}
		}

		public static SagePurchaseInvoicesList GetPurchaseInvoices(string accessToken, DateTime? fromDate)
		{
			List<SagePurchaseInvoice> purchaseInvoices = ExecuteRequestAndGetDeserializedResponse<SagePurchaseInvoiceDeserialization, SagePurchaseInvoice>(accessToken, config.PurchaseInvoicesRequest, fromDate, CreateDeserializedPurchaseInvoices, SageDesreializer.DeserializePurchaseInvoice);
			var purchaseInvoicesList = new SagePurchaseInvoicesList(DateTime.UtcNow, purchaseInvoices);
			return purchaseInvoicesList;
		}

		private static PaginatedResults<SagePurchaseInvoiceDeserialization> CreateDeserializedPurchaseInvoices(string cleanResponse)
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

		private static PaginatedResults<SagePurchaseInvoiceDeserialization> DeserializePurchaseInvoices(string cleanResponse)
		{
			try
			{
				var js = new JavaScriptSerializer();
				return ((PaginatedResults<SagePurchaseInvoiceDeserialization>)js.Deserialize(cleanResponse, typeof(PaginatedResults<SagePurchaseInvoiceDeserialization>)));
			}
			catch (Exception e)
			{
				log.ErrorFormat("Failed deserializing sage purchase invoices response:{0}. The error was:{1}", cleanResponse, e);
				return null;
			}
		}

		public static SageIncomesList GetIncomes(string accessToken, DateTime? fromDate)
		{
			List<SageIncome> incomes = ExecuteRequestAndGetDeserializedResponse<SageIncomeDeserialization, SageIncome>(accessToken, config.IncomesRequest, fromDate, CreateDeserializedIncomes, SageDesreializer.DeserializeIncome);
			var incomesList = new SageIncomesList(DateTime.UtcNow, incomes);
			return incomesList;
		}

		private static PaginatedResults<SageIncomeDeserialization> CreateDeserializedIncomes(string cleanResponse)
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

		private static PaginatedResults<SageIncomeDeserialization> DeserializeIncomes(string cleanResponse)
		{
			try
			{
				var js = new JavaScriptSerializer();
				return ((PaginatedResults<SageIncomeDeserialization>)js.Deserialize(cleanResponse, typeof(PaginatedResults<SageIncomeDeserialization>)));
			}
			catch (Exception e)
			{
				log.ErrorFormat("Failed deserializing sage incomes response:{0}. The error was:{1}", cleanResponse, e);
				return null;
			}
		}

		public static SageExpendituresList GetExpenditures(string accessToken, DateTime? fromDate)
		{
			List<SageExpenditure> expenditures = ExecuteRequestAndGetDeserializedResponse<SageExpenditureDeserialization, SageExpenditure>(accessToken, config.ExpendituresRequest, fromDate, CreateDeserializedExpenditures, SageDesreializer.DeserializeExpenditure);
			var expendituresList = new SageExpendituresList(DateTime.UtcNow, expenditures);
			return expendituresList;
		}

		private static PaginatedResults<SageExpenditureDeserialization> CreateDeserializedExpenditures(string cleanResponse)
		{
			var deserializeExpenditures = DeserializeExpenditures(cleanResponse);
			if (deserializeExpenditures == null)
			{
				log.Error("Error deserializing sage expenditures");
				return null;
			}
			if (deserializeExpenditures.diagnoses != null)
			{
				foreach (SageDiagnostic diagnostic in deserializeExpenditures.diagnoses)
				{
					log.ErrorFormat("Error occured during sage expenditures request. Expenditures were not fetched. Message:{0} Source:{1} Severity:{2} DataCode:{3}",
						diagnostic.message, diagnostic.source, diagnostic.severity, diagnostic.dataCode);
				}
				return null;
			}

			return deserializeExpenditures;
		}

		private static PaginatedResults<SageExpenditureDeserialization> DeserializeExpenditures(string cleanResponse)
		{
			try
			{
				var js = new JavaScriptSerializer();
				return ((PaginatedResults<SageExpenditureDeserialization>)js.Deserialize(cleanResponse, typeof(PaginatedResults<SageExpenditureDeserialization>)));
			}
			catch (Exception e)
			{
				log.ErrorFormat("Failed deserializing sage expenditures response:{0}. The error was:{1}", cleanResponse, e);
				return null;
			}
		}

		private static List<TConverted> ExecuteRequestAndGetDeserializedResponse<TDeserialize, TConverted>(string accessToken, string requestUrl, DateTime? fromDate, Func<string, PaginatedResults<TDeserialize>> deserializeFunction, Func<TDeserialize, TConverted> conversionFunc)
			where TDeserialize : class
		{
			string authorizationHeader = string.Format("Bearer {0}", accessToken);
			string monthPart = !fromDate.HasValue ? string.Empty : string.Format("?{0}", string.Format(config.RequestForDatesPart, fromDate.Value.ToString("dd/MM/yyyy"), DateTime.UtcNow.ToString("dd/MM/yyyy")).Replace('-', '/'));
			string fullRequest = string.Format("{0}{1}", requestUrl, monthPart);
			var request = new RestRequest(Method.GET) { Resource = fullRequest };
			request.AddHeader("Authorization", authorizationHeader);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			PaginatedResults<TDeserialize> deserializedResponse = deserializeFunction(CleanResponse(response.Content));
			if (deserializedResponse == null)
			{
				log.Error("Sage response deserialization failed");
				return new List<TConverted>();
			}

			var results = new List<TConverted>();
			FillFromDeserializedData(deserializedResponse, results, conversionFunc);

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
					return new List<TConverted>();
				}

				FillFromDeserializedData(deserializedResponse, results, conversionFunc);

				nextUrl = GetNextUrl(deserializedResponse, fullRequest);
			}

			return results;
		}

		private static void FillFromDeserializedData<TDeserialize, TConverted>(PaginatedResults<TDeserialize> deserializedResponse, List<TConverted> list, Func<TDeserialize, TConverted> conversionFunc)
		{
			foreach (var deserializePaymentStatus in deserializedResponse.resources)
			{
				TryConvertDeserialized(list, deserializePaymentStatus, conversionFunc);
			}
		}

		private static void TryConvertDeserialized<TConverted, TDeserialization>(List<TConverted> list, TDeserialization deserializaedObject, Func<TDeserialization, TConverted> convertDeserializedObjectFunc)
		{
			try
			{
				list.Add(convertDeserializedObjectFunc(deserializaedObject));
			}
			catch (Exception)
			{
				log.ErrorFormat("Failed creating {0} for object:{1}. Object won't be handled!", typeof(TConverted), deserializaedObject);
			}
		}

		private static string CleanResponse(string originalResponse)
		{
			return originalResponse.Replace("\"$", "\"");
		}

		private static string GetNextUrl<T>(PaginatedResults<T> pagenatedResults, string request)
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