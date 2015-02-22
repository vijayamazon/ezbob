namespace Sage {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Web.Script.Serialization;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using RestSharp;

	public class SageConnector {
		public static List<SagePaymentStatus> GetPaymentStatuses(string accessToken) {
			return ExecuteRequestAndGetDeserializedResponse<SagePaymentStatusDeserialization, SagePaymentStatus>(accessToken, config.PaymentStatusesRequest, null, SageDesreializer.DeserializePaymentStatus);
		}

		public static SageSalesInvoicesList GetSalesInvoices(string accessToken, DateTime? fromDate) {
			List<SageSalesInvoice> salesInvoices = ExecuteRequestAndGetDeserializedResponse<SageSalesInvoiceDeserialization, SageSalesInvoice>(accessToken, config.SalesInvoicesRequest, fromDate, SageDesreializer.DeserializeSalesInvoice);
			var salesInvoicesList = new SageSalesInvoicesList(DateTime.UtcNow, salesInvoices);
			return salesInvoicesList;
		}

		public static SagePurchaseInvoicesList GetPurchaseInvoices(string accessToken, DateTime? fromDate) {
			List<SagePurchaseInvoice> purchaseInvoices = ExecuteRequestAndGetDeserializedResponse<SagePurchaseInvoiceDeserialization, SagePurchaseInvoice>(accessToken, config.PurchaseInvoicesRequest, fromDate, SageDesreializer.DeserializePurchaseInvoice);
			var purchaseInvoicesList = new SagePurchaseInvoicesList(DateTime.UtcNow, purchaseInvoices);
			return purchaseInvoicesList;
		}

		public static SageIncomesList GetIncomes(string accessToken, DateTime? fromDate) {
			List<SageIncome> incomes = ExecuteRequestAndGetDeserializedResponse<SageIncomeDeserialization, SageIncome>(accessToken, config.IncomesRequest, fromDate, SageDesreializer.DeserializeIncome);
			var incomesList = new SageIncomesList(DateTime.UtcNow, incomes);
			return incomesList;
		}

		public static SageExpendituresList GetExpenditures(string accessToken, DateTime? fromDate) {
			List<SageExpenditure> expenditures = ExecuteRequestAndGetDeserializedResponse<SageExpenditureDeserialization, SageExpenditure>(accessToken, config.ExpendituresRequest, fromDate, SageDesreializer.DeserializeExpenditure);
			var expendituresList = new SageExpendituresList(DateTime.UtcNow, expenditures);
			return expendituresList;
		}

		private static List<TConverted> ExecuteRequestAndGetDeserializedResponse<TDeserialize, TConverted>(string accessToken, string requestUrl, DateTime? fromDate, Func<TDeserialize, TConverted> conversionFunc)
			where TDeserialize : class {
			string authorizationHeader = string.Format("Bearer {0}", accessToken);
			string monthPart = !fromDate.HasValue ? string.Empty : string.Format("?{0}", string.Format(config.RequestForDatesPart, fromDate.Value.ToString("dd/MM/yyyy"), DateTime.UtcNow.ToString("dd/MM/yyyy")).Replace('-', '/'));
			string fullRequest = string.Format("{0}{1}", requestUrl, monthPart);
			var request = new RestRequest(Method.GET) { Resource = fullRequest };
			request.AddHeader("Authorization", authorizationHeader);

			var client = new RestClient(new Uri(SageConfig.SageBaseUri));

			log.Info("Making sage request:{0}", fullRequest);
			IRestResponse response = client.Execute(request);
			PaginatedResults<TDeserialize> deserializedResponse = CreateDeserializedItems<TDeserialize>(CleanResponse(response.Content, typeof(TDeserialize).Name));
			if (deserializedResponse == null) {
				log.Error("Sage response deserialization failed");
				return new List<TConverted>();
			}

			log.Info("Successfully serialized sage response");

			var results = new List<TConverted>();
			FillFromDeserializedData(deserializedResponse, results, conversionFunc);

			string nextUrl = GetNextUrl(deserializedResponse, fullRequest);

			while (nextUrl != null) {
				request = new RestRequest(Method.GET) { Resource = nextUrl };
				request.AddHeader("Authorization", authorizationHeader);
				log.Info("Making another sage request:{0}", nextUrl);
				response = client.Execute(request);

				deserializedResponse = CreateDeserializedItems<TDeserialize>(CleanResponse(response.Content, typeof(TDeserialize).Name));
				if (deserializedResponse == null) {
					log.Error("Sage response deserialization failed");
					return new List<TConverted>();
				}

				FillFromDeserializedData(deserializedResponse, results, conversionFunc);

				nextUrl = GetNextUrl(deserializedResponse, fullRequest);
			}

			log.Info("Finished retreiving sage request");

			return results;
		}

		public static PaginatedResults<TDeserialized> CreateDeserializedItems<TDeserialized>(string cleanResponse) {
			var deserializedItems = DeserializeItems<TDeserialized>(cleanResponse);
			if (deserializedItems == null) {
				log.Error("Error deserializing sage {0}", typeof(TDeserialized));
				return null;
			}
			if (deserializedItems.diagnoses != null) {
				foreach (SageDiagnostic diagnostic in deserializedItems.diagnoses) {
					log.Error("Error occured during sage {0} request. Message:{1} Source:{2} Severity:{3} DataCode:{4}",
						typeof(TDeserialized).Name, diagnostic.message, diagnostic.source, diagnostic.severity, diagnostic.dataCode);
				}
				return null;
			}

			return deserializedItems;
		}

		private static PaginatedResults<TDeserialized> DeserializeItems<TDeserialized>(string cleanResponse) {
			try {
				var js = new JavaScriptSerializer();
				var deserializedObject = ((PaginatedResults<TDeserialized>)js.Deserialize(cleanResponse, typeof(PaginatedResults<TDeserialized>)));
				if (deserializedObject == null || (deserializedObject.resources == null)) {
					log.Error("Error deserializing response:{0}", cleanResponse);
					throw new Exception();
				}
				return deserializedObject;
			} catch (Exception e) {
				log.Error(e, @"Failed deserializing sage {0} response:
					
					{1}
		
					", typeof(TDeserialized), cleanResponse);
				return null;
			}
		}

		private static void FillFromDeserializedData<TDeserialize, TConverted>(PaginatedResults<TDeserialize> deserializedResponse, List<TConverted> list, Func<TDeserialize, TConverted> conversionFunc) {
			foreach (var deserializePaymentStatus in deserializedResponse.resources) {
				TryConvertDeserialized(list, deserializePaymentStatus, conversionFunc);
			}
		}

		private static void TryConvertDeserialized<TConverted, TDeserialization>(List<TConverted> list, TDeserialization deserializaedObject, Func<TDeserialization, TConverted> convertDeserializedObjectFunc) {
			try {
				list.Add(convertDeserializedObjectFunc(deserializaedObject));
			} catch (Exception e) {
				log.Error(e, "Failed creating {0} for object:{1}. Object won't be handled!", typeof(TConverted), deserializaedObject);
			}
		}

		public static string CleanResponse(string originalResponse, string type) {
			log.Debug("original response {1} \n {0}", originalResponse, type);
			return originalResponse.Replace("\"$", "\"");
		}

		private static string GetNextUrl<T>(PaginatedResults<T> pagenatedResults, string request) {
			int nextIndex = pagenatedResults.startIndex + pagenatedResults.itemsPerPage;
			if (pagenatedResults.totalResults <= nextIndex) {
				return null;
			}

			return string.Format("{0}{1}$startIndex={2}", request, request.Contains("?") ? "&" : "?", nextIndex);
		}

		public static AccessTokenContainer GetToken(string code, string redirectVal, out string errorMessage) {
			var request = new RestRequest(Method.POST) { Resource = config.OAuthTokenRequestPath, };
			request.AddHeader("Accept", "application / json");

			request.AddParameter("grant_type", "authorization_code");
			request.AddParameter("scope", string.Empty);
			request.AddParameter("code", code);
			request.AddParameter("redirect_uri", redirectVal);
			request.AddParameter("client_secret", config.OAuthSecret);
			request.AddParameter("client_id", config.OAuthIdentifier);

			var client = new RestClient(config.OAuthTokenRequestHost);

			errorMessage = null;

			try {
				IRestResponse response = client.Execute(request);

				if (response != null) {
					if (response.StatusCode != HttpStatusCode.OK) {
						log.Error(
							"Not parsing for access token: status code is {2} ({3}).\n\tRequest:\n\t{0}\n\tResponse:\n\t{1}",
							config.OAuthTokenRequestHost + "/" + request.Resource,
							response.Content,
							response.StatusCode,
							(int)response.StatusCode
						);

						return null;
					} // if

					log.Debug(
						"Just before parsing access token.\n\tRequest:\n\t{0}\n\tResponse:\n\t{1}",
						config.OAuthTokenRequestHost + "/" + request.Resource,
						response.Content
					);

					var js = new JavaScriptSerializer();

					var deserializedResponse = (AccessTokenContainer)js.Deserialize(
						response.Content,
						typeof(AccessTokenContainer)
					);

					if (deserializedResponse != null && deserializedResponse.access_token != null)
						return deserializedResponse;

					log.Error(
						"Failed parsing access token.\n\tRequest:{0}\n\tResponse:{1}",
						request.Resource,
						response.Content
					);

					throw new Exception("Failed getting token but parsing didn't threw exception");
				} // if
			} catch (Exception e) {
				errorMessage = "Failure getting access token";
				log.Warn(e, "{0}", errorMessage);
			} // try

			return null;
		} // GetToken

		public static string GetApprovalRequest(string redirectUri) {
			return string.Format(
				"{3}/{0}?redirect_uri={1}&response_type=code&client_id={2}",
				config.OAuthApprovalRequestPath,
				redirectUri,
				config.OAuthIdentifier,
				config.OAuthApprovalRequestHost
			);
		} // GetApprovalRequest

		private static readonly SageConfig config = new SageConfig();
		private static readonly ASafeLog log = new SafeILog(typeof(SageConnector));
	} // class SageConnector
} // namespace