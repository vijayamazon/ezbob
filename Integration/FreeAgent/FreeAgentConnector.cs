namespace FreeAgent {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Web.Script.Serialization;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using RestSharp;

	public class FreeAgentConnector {
		private static readonly FreeAgentConfig config = new FreeAgentConfig();
		private static readonly ASafeLog log = new SafeILog(typeof(FreeAgentConnector));

		private static bool TryToDeserialize<T>(out T deserializedResult, string responseContent) where T : class {
			try {
				var js = new JavaScriptSerializer();
				deserializedResult = ((T)js.Deserialize(responseContent, typeof(T)));
			} catch (Exception e) {
				log.Error("Failed parsing response from FreeAgent to type:{0} with exception:{1}. The response:{2}", typeof(T), e, responseContent);
				deserializedResult = null;
				return false;
			}

			return true;
		}

		public static FreeAgentInvoicesList GetInvoices(string accessToken, int numOfMonths) {
			string monthPart = numOfMonths == -1
				? string.Empty
				: string.Format(config.InvoicesRequestMonthPart, numOfMonths);

			string timedInvoicesRequest = string.Format(
				"{0}{1}",
				"/v2/invoices?nested_invoice_items=true"/* config.InvoicesRequest */,
				monthPart
			);

			HttpClient client = new HttpClient { BaseAddress = new Uri("https://api.freeagent.com") };

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, timedInvoicesRequest);

			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
			request.Headers.UserAgent.Add(new ProductInfoHeaderValue("EzbobFreeAgentConnector", "2.0"));

			int batchNo = 1;

			log.Debug(
				"Executing invoices batch #{0} {1} request:\nURL: {2}\nHeaders:\n\t{3}",
				batchNo,
				request.Method,
				request.RequestUri,
				string.Join("\n\t", request.Headers.Select(pair => string.Format(
					"{0}: {1}",
					pair.Key,
					string.Join(", ", pair.Value)
				)))
			);

			HttpResponseMessage response = client.SendAsync(request).Result;

			string responseContent = response.Content.ReadAsStringAsync().Result;

			log.Debug(
				"Invoices batch #{0} response status code is {1}, content:\n{2}",
				batchNo,
				response.StatusCode,
				responseContent
			);

			var invoices = new List<FreeAgentInvoice>();

			InvoicesListHelper deserializedResponse;

			bool deserializeSuccess =
				TryToDeserialize(out deserializedResponse, responseContent) &&
				(deserializedResponse != null) &&
				(deserializedResponse.Invoices != null);

			if (deserializeSuccess) {
				log.Debug("{0} invoices fetched from batch #{1}.", deserializedResponse.Invoices.Count, batchNo);
				invoices.AddRange(deserializedResponse.Invoices);
			} else
				log.Error("Failed to parse invoices from batch #{0}.", batchNo);

			client.Dispose();

			var freeAgentInvoicesList = new FreeAgentInvoicesList(DateTime.UtcNow, invoices);

			return freeAgentInvoicesList;
		} // GetInvoices

		public static FreeAgentInvoicesList GetInvoicesOld(string accessToken, int numOfMonths) {
			string monthPart = numOfMonths == -1
				? string.Empty
				: string.Format(config.InvoicesRequestMonthPart, numOfMonths);

			Dictionary<string, string> headers = new Dictionary<string, string> {
				{ "Authorization", "Bearer " + accessToken },
			};

			string timedInvoicesRequest = string.Format("{0}{1}", config.InvoicesRequest, monthPart);

			var request = new RestRequest(Method.GET) { Resource = timedInvoicesRequest };

			foreach (var pair in headers)
				request.AddHeader(pair.Key, pair.Value);

			var client = new RestClient();

			int batchNo = 1;

			log.Debug(
				"Executing invoices batch #{0} {1} request:\nURL: {2}\nHeaders:\n\t{3}",
				batchNo,
				request.Method,
				timedInvoicesRequest,
				string.Join("\n\t", headers.Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value)))
			);

			IRestResponse response = client.Execute(request);

			log.Debug(
				"Invoices batch #{0} response:\n" +
				"Content length is {1};\n" +
				"Status code is {2};\n" +
				"Response status is {3}.",
				batchNo,
				response.Content.Length,
				response.StatusCode,
				response.ResponseStatus
			);

			var invoices = new List<FreeAgentInvoice>();

			InvoicesListHelper deserializedResponse;

			bool deserializeSuccess =
				TryToDeserialize(out deserializedResponse, response.Content) &&
				(deserializedResponse != null) &&
				(deserializedResponse.Invoices != null);

			if (deserializeSuccess) {
				log.Debug("{0} invoices fetched from batch #{1}.", deserializedResponse.Invoices.Count, batchNo);
				invoices.AddRange(deserializedResponse.Invoices);
			} else
				log.Error("Failed to parse invoices from batch #{0}.", batchNo);

			string nextUrl = GetNextUrl(response);

			while (nextUrl != null) {
				batchNo++;

				if (!nextUrl.Contains("nested_invoice_items")) {
					// This is done to workaround an issue in FreeAgent's API (The pagination removes this parameter).
					nextUrl = nextUrl.Replace("?", "?nested_invoice_items=true&");
				} // if

				request = new RestRequest(Method.GET) { Resource = nextUrl };

				foreach (var pair in headers)
					request.AddHeader(pair.Key, pair.Value);

				log.Debug(
					"Executing invoices batch #{0} {1} request:\nURL: {2}\nHeaders:\n\t{3}",
					batchNo,
					request.Method,
					timedInvoicesRequest,
					string.Join("\n\t", headers.Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value)))
				);

				response = client.Execute(request);

				log.Debug("Invoices batch #{0} response content length is {1}.", batchNo, response.Content.Length);

				deserializeSuccess =
					TryToDeserialize(out deserializedResponse, response.Content) &&
					(deserializedResponse != null) &&
					(deserializedResponse.Invoices != null);

				if (deserializeSuccess) {
					log.Debug("{0} invoices fetched from batch #{1}.", deserializedResponse.Invoices.Count, batchNo);
					invoices.AddRange(deserializedResponse.Invoices);
				} else
					log.Error("Failed to parse invoices from batch #{0}.", batchNo);

				nextUrl = GetNextUrl(response);
			} // while

			log.Debug("{0} invoices fetched in total from {1} batch(es).", invoices.Count, batchNo);

			var freeAgentInvoicesList = new FreeAgentInvoicesList(DateTime.UtcNow, invoices);

			return freeAgentInvoicesList;
		} // GetInvoices

		public static FreeAgentCompany GetCompany(string accessToken) {
			var request = new RestRequest(Method.GET) { Resource = config.CompanyRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			FreeAgentCompanyList deserializedResponse;
			if (TryToDeserialize(out deserializedResponse, response.Content) && deserializedResponse != null && deserializedResponse.Company != null) {
				return deserializedResponse.Company;
			}

			log.Error("Failed parsing company. Request:{0} Response:{1}", request.Resource, response.Content);
			return new FreeAgentCompany {
				company_start_date = new DateTime(1900, 1, 1),
				freeagent_start_date = new DateTime(1900, 1, 1),
				first_accounting_year_end = new DateTime(1900, 1, 1),
				name = "Can't get company's name"
			};
		}

		public static List<FreeAgentUsers> GetUsers(string accessToken) {
			var request = new RestRequest(Method.GET) { Resource = config.UsersRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);

			FreeAgentUsersList deserializedResponse;
			if (TryToDeserialize(out deserializedResponse, response.Content) && deserializedResponse != null && deserializedResponse.Users != null) {
				return deserializedResponse.Users;
			}

			log.Error("Failed parsing users. Request:{0} Response:{1}", request.Resource, response.Content);
			return new List<FreeAgentUsers>();
		}

		public static FreeAgentExpensesList GetExpenses(string accessToken, DateTime? fromDate) {
			string fromDatePart = fromDate == null ? string.Empty : string.Format(config.ExpensesRequestDatePart, fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day);
			string expensesRequest = string.Format("{0}{1}", config.ExpensesRequest, fromDatePart);
			var request = new RestRequest(Method.GET) { Resource = expensesRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			var expenses = new List<FreeAgentExpense>();
			ExpensesListHelper deserializedResponse;
			if (TryToDeserialize(out deserializedResponse, response.Content) && deserializedResponse != null && deserializedResponse.Expenses != null) {
				expenses.AddRange(deserializedResponse.Expenses);
			} else {
				log.Error("Failed parsing expenses. Request:{0} Response:{1}", request.Resource, response.Content);
			}

			string nextUrl = GetNextUrl(response);

			while (nextUrl != null) {
				request = new RestRequest(Method.GET) { Resource = nextUrl };
				request.AddHeader("Authorization", "Bearer " + accessToken);
				response = client.Execute(request);

				if (TryToDeserialize(out deserializedResponse, response.Content) && deserializedResponse != null && deserializedResponse.Expenses != null) {
					expenses.AddRange(deserializedResponse.Expenses);
				} else {
					log.Error("Failed parsing expenses. Request:{0} Response:{1}", request.Resource, response.Content);
				}

				nextUrl = GetNextUrl(response);
			}

			var freeAgentExpenesList = new FreeAgentExpensesList(DateTime.UtcNow, expenses);
			return freeAgentExpenesList;
		}

		public static FreeAgentExpenseCategory GetExpenseCategory(string accessToken, string categoryUrl) {
			var request = new RestRequest(Method.GET) { Resource = categoryUrl };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);

			ExpenseCategoriesListHelper deserializedResponse;
			if (TryToDeserialize(out deserializedResponse, response.Content) && deserializedResponse != null && deserializedResponse.Category != null) {
				return deserializedResponse.Category;
			}

			log.Error("Failed parsing category. Request:{0} Response:{1}", request.Resource, response.Content);
			return new FreeAgentExpenseCategory { url = string.Empty };
		}

		private static string GetNextUrl(IRestResponse response) {
			try {
				foreach (var header in response.Headers) {
					if (header.Name == "Link") {
						var paginationParts = header.Value.ToString().Split(',');
						foreach (var paginationPart in paginationParts) {
							if (paginationPart.Contains("rel='next'")) {
								var nextUrlPart = paginationPart.Split(';')[0];
								return nextUrlPart.Substring(1, nextUrlPart.Length - 2);
							}
						}
					}
				}
			} catch (Exception) {
				return null;
			}
			return null;
		}

		public static AccessTokenContainer GetToken(string code, string redirectVal, out string errorMessage) {
			string accessTokenRequest =
				string.Format("{0}?grant_type=authorization_code&code={1}&redirect_uri={2}&scope=&client_secret={3}&client_id={4}",
							  config.OAuthTokenEndpoint, code, redirectVal, config.OAuthSecret,
							  config.OAuthIdentifier);
			var request = (HttpWebRequest)WebRequest.Create(accessTokenRequest);
			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";
			errorMessage = null;
			try {
				using (var twitpicResponse = (HttpWebResponse)request.GetResponse()) {
					var response = twitpicResponse.GetResponseStream();
					if (response != null) {
						using (var reader = new StreamReader(response)) {
							var js = new JavaScriptSerializer();
							var objText = reader.ReadToEnd();

							var deserializedResponse = (AccessTokenContainer)js.Deserialize(objText, typeof(AccessTokenContainer));
							if (deserializedResponse != null && deserializedResponse.access_token != null) {
								return deserializedResponse;
							}
							log.Error("Failed parsing access token. Request:{0} Response:{1}", request.RequestUri, objText);
							throw new Exception("Failed getting token but parsing didn't threw exception");
						}
					}
				}
			} catch (Exception e) {
				errorMessage = "Failure getting access token";
				log.Warn("{0}. Exception:{1}", errorMessage, e);
			}
			return null;
		}

		public static AccessTokenContainer RefreshToken(string refreshToken) {
			string accessTokenRequest = string.Format("{0}?grant_type=refresh_token&refresh_token={1}&client_secret={2}&client_id={3}",
				config.OAuthTokenEndpoint, refreshToken, config.OAuthSecret, config.OAuthIdentifier);
			var request = (HttpWebRequest)WebRequest.Create(accessTokenRequest);
			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";
			try {
				using (var twitpicResponse = (HttpWebResponse)request.GetResponse()) {
					var response = twitpicResponse.GetResponseStream();
					if (response != null) {
						using (var reader = new StreamReader(response)) {
							var js = new JavaScriptSerializer();
							var objText = reader.ReadToEnd();

							var deserializedResponse = (AccessTokenContainer)js.Deserialize(objText, typeof(AccessTokenContainer));
							if (deserializedResponse != null && deserializedResponse.access_token != null) {
								deserializedResponse.refresh_token = refreshToken;
								return deserializedResponse;
							}
							log.Error("Failed refreshing access token. Request:{0} Response:{1}", request.RequestUri, objText);
							throw new Exception("Failed refreshing token but parsing didn't threw exception");
						}
					}
				}
			} catch (Exception e) {
				log.Warn("Failed refreshing token. Exception:{0}", e);
			}

			return null;
		}

		public static string GetApprovalRequest(string redirectUri) {
			return string.Format("{0}?redirect_uri={1}&response_type=code&client_id={2}",
								 config.OAuthAuthorizationEndpoint, redirectUri, config.OAuthIdentifier);
		}
	}
}