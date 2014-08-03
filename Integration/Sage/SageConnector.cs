namespace Sage
{
    using System;
    using System.Collections.Generic;
    using System.Web.Script.Serialization;
    using EZBob.DatabaseLib.DatabaseWrapper.Order;
    using RestSharp;
	using log4net;
	
	public class SageConnector
	{
		private static readonly SageConfig config = new SageConfig();
		private static readonly ILog log = LogManager.GetLogger(typeof(SageConnector));

		public static List<SagePaymentStatus> GetPaymentStatuses(string accessToken)
		{
			return ExecuteRequestAndGetDeserializedResponse<SagePaymentStatusDeserialization, SagePaymentStatus>(accessToken, config.PaymentStatusesRequest, null, SageDesreializer.DeserializePaymentStatus);
		}

		public static SageSalesInvoicesList GetSalesInvoices(string accessToken, DateTime? fromDate)
		{
			List<SageSalesInvoice> salesInvoices = ExecuteRequestAndGetDeserializedResponse<SageSalesInvoiceDeserialization, SageSalesInvoice>(accessToken, config.SalesInvoicesRequest, fromDate, SageDesreializer.DeserializeSalesInvoice);
			var salesInvoicesList = new SageSalesInvoicesList(DateTime.UtcNow, salesInvoices);
			return salesInvoicesList;
		}

		public static SagePurchaseInvoicesList GetPurchaseInvoices(string accessToken, DateTime? fromDate)
		{
			List<SagePurchaseInvoice> purchaseInvoices = ExecuteRequestAndGetDeserializedResponse<SagePurchaseInvoiceDeserialization, SagePurchaseInvoice>(accessToken, config.PurchaseInvoicesRequest, fromDate, SageDesreializer.DeserializePurchaseInvoice);
			var purchaseInvoicesList = new SagePurchaseInvoicesList(DateTime.UtcNow, purchaseInvoices);
			return purchaseInvoicesList;
		}

		public static SageIncomesList GetIncomes(string accessToken, DateTime? fromDate)
		{
			List<SageIncome> incomes = ExecuteRequestAndGetDeserializedResponse<SageIncomeDeserialization, SageIncome>(accessToken, config.IncomesRequest, fromDate, SageDesreializer.DeserializeIncome);
			var incomesList = new SageIncomesList(DateTime.UtcNow, incomes);
			return incomesList;
		}

		public static SageExpendituresList GetExpenditures(string accessToken, DateTime? fromDate)
		{
			List<SageExpenditure> expenditures = ExecuteRequestAndGetDeserializedResponse<SageExpenditureDeserialization, SageExpenditure>(accessToken, config.ExpendituresRequest, fromDate, SageDesreializer.DeserializeExpenditure);
			var expendituresList = new SageExpendituresList(DateTime.UtcNow, expenditures);
			return expendituresList;
		}

		private static List<TConverted> ExecuteRequestAndGetDeserializedResponse<TDeserialize, TConverted>(string accessToken, string requestUrl, DateTime? fromDate, Func<TDeserialize, TConverted> conversionFunc)
			where TDeserialize : class
		{
			string authorizationHeader = string.Format("Bearer {0}", accessToken);
			string monthPart = !fromDate.HasValue ? string.Empty : string.Format("?{0}", string.Format(config.RequestForDatesPart, fromDate.Value.ToString("dd/MM/yyyy"), DateTime.UtcNow.ToString("dd/MM/yyyy")).Replace('-', '/'));
			string fullRequest = string.Format("{0}{1}", requestUrl, monthPart);
			var request = new RestRequest(Method.GET) { Resource = fullRequest };
			request.AddHeader("Authorization", authorizationHeader);

			var client = new RestClient();

			log.InfoFormat("Making sage request:{0}", fullRequest);
			IRestResponse response = client.Execute(request);
			PaginatedResults<TDeserialize> deserializedResponse = CreateDeserializedItems<TDeserialize>(CleanResponse(response.Content));
			if (deserializedResponse == null)
			{
				log.Error("Sage response deserialization failed");
				return new List<TConverted>();
			}
			
			log.Info("Successfully serialized sage response");

			var results = new List<TConverted>();
			FillFromDeserializedData(deserializedResponse, results, conversionFunc);

			string nextUrl = GetNextUrl(deserializedResponse, fullRequest);

			while (nextUrl != null)
			{
				request = new RestRequest(Method.GET) { Resource = nextUrl };
				request.AddHeader("Authorization", authorizationHeader);
				log.InfoFormat("Making another sage request:{0}", nextUrl);
				response = client.Execute(request);

				deserializedResponse = CreateDeserializedItems<TDeserialize>(CleanResponse(response.Content));
				if (deserializedResponse == null)
				{
					log.Error("Sage response deserialization failed");
					return new List<TConverted>();
				}

				FillFromDeserializedData(deserializedResponse, results, conversionFunc);

				nextUrl = GetNextUrl(deserializedResponse, fullRequest);
			}

			log.Info("Finished retreiving sage request");

			return results;
		}

		public static PaginatedResults<TDeserialized> CreateDeserializedItems<TDeserialized>(string cleanResponse)
		{
			var deserializedItems = DeserializeItems<TDeserialized>(cleanResponse);
			if (deserializedItems == null)
			{
				log.ErrorFormat("Error deserializing sage {0}", typeof(TDeserialized));
				return null;
			}
			if (deserializedItems.diagnoses != null)
			{
				foreach (SageDiagnostic diagnostic in deserializedItems.diagnoses)
				{
					log.ErrorFormat("Error occured during sage {0} request. Message:{1} Source:{2} Severity:{3} DataCode:{4}",
						typeof(TDeserialized), diagnostic.message, diagnostic.source, diagnostic.severity, diagnostic.dataCode);
				}
				return null;
			}

			return deserializedItems;
		}

		private static PaginatedResults<TDeserialized> DeserializeItems<TDeserialized>(string cleanResponse)
		{
			try
			{
				var js = new JavaScriptSerializer();
				var deserializedObject = ((PaginatedResults<TDeserialized>)js.Deserialize(cleanResponse, typeof(PaginatedResults<TDeserialized>)));
				if (deserializedObject == null || (deserializedObject.resources == null && deserializedObject.diagnoses == null))
				{
					string errorMessage = string.Format("Error deserializing response:{0}", cleanResponse);
					log.ErrorFormat(errorMessage);
					throw new Exception();
				}
				return deserializedObject;
			}
			catch (Exception e)
			{
				log.ErrorFormat("Failed deserializing sage {0} response:{1}. The error was:{2}", typeof(TDeserialized), cleanResponse, e);
				return null;
			}
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

		public static string CleanResponse(string originalResponse)
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
					var deserializedResponse = (AccessTokenContainer)js.Deserialize(response.Content, typeof(AccessTokenContainer));
					if (deserializedResponse != null && deserializedResponse.access_token != null)
					{
						return deserializedResponse;
					}
					log.ErrorFormat("Failed parsing access token. Request:{0} Response:{1}", request.Resource, response.Content);
					throw new Exception("Failed getting token but parsing didn't threw exception"); 
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