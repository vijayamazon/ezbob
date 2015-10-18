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

	public class FreeAgentConnector : IDisposable {
		public FreeAgentConnector() {
			this.serializer = new JavaScriptSerializer();
			this.config = new FreeAgentConfig();
			this.client = new HttpClient { BaseAddress = new Uri("https://api.freeagent.com") };
		} // constructor

		public FreeAgentInvoicesList GetInvoices(string accessToken, int numOfMonths) {
			string monthPart = numOfMonths == -1
				? string.Empty
				: string.Format(this.config.InvoicesRequestMonthPart, numOfMonths);

			string timedInvoicesRequest = string.Format(
				"{0}{1}",
				"/v2/invoices?nested_invoice_items=true"/* this.config.InvoicesRequest */,
				monthPart
			);

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

			HttpResponseMessage response = this.client.SendAsync(request).Result;

			string responseContent = response.Content.ReadAsStringAsync().Result;

			log.Debug(
				"Invoices batch #{0} response status code is {1}, content:\n{2}",
				batchNo,
				response.StatusCode,
				responseContent
			);

			var invoices = new List<FreeAgentInvoice>();

			InvoicesListHelper deserializedResponse = DeserializeResponse<InvoicesListHelper>(responseContent);

			if ((deserializedResponse != null) && (deserializedResponse.Invoices != null)) {
				log.Debug("{0} invoices fetched from batch #{1}.", deserializedResponse.Invoices.Count, batchNo);
				invoices.AddRange(deserializedResponse.Invoices);
			} else
				log.Error("Failed to parse invoices from batch #{0}.", batchNo);

			log.Debug("Batch #{0} response headers - begin", batchNo);
			foreach (var header in response.Headers)
				log.Debug("Batch #{0} response header {1}: {2}", batchNo, header.Key, string.Join(" ;;; ", header.Value));
			log.Debug("Batch #{0} response headers - end", batchNo);

			var freeAgentInvoicesList = new FreeAgentInvoicesList(DateTime.UtcNow, invoices);

			return freeAgentInvoicesList;
		} // GetInvoices

		public FreeAgentInvoicesList GetInvoicesOld(string accessToken, int numOfMonths) {
			string monthPart = numOfMonths == -1
				? string.Empty
				: string.Format(this.config.InvoicesRequestMonthPart, numOfMonths);

			Dictionary<string, string> headers = new Dictionary<string, string> {
				{ "Authorization", "Bearer " + accessToken },
			};

			string timedInvoicesRequest = string.Format("{0}{1}", this.config.InvoicesRequest, monthPart);

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

			InvoicesListHelper deserializedResponse = DeserializeResponse<InvoicesListHelper>(response.Content);

			if ((deserializedResponse != null) && (deserializedResponse.Invoices != null)) {
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

				deserializedResponse = DeserializeResponse<InvoicesListHelper>(response.Content);

				if ((deserializedResponse != null) && (deserializedResponse.Invoices != null)) {
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

		public FreeAgentCompany GetCompany(string accessToken) {
			var request = new RestRequest(Method.GET) { Resource = this.config.CompanyRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			FreeAgentCompanyList deserializedResponse = DeserializeResponse<FreeAgentCompanyList>(response.Content);

			if (deserializedResponse != null && deserializedResponse.Company != null)
				return deserializedResponse.Company;

			log.Error("Failed parsing company. Request:{0} Response:{1}", request.Resource, response.Content);

			return new FreeAgentCompany {
				company_start_date = new DateTime(1900, 1, 1),
				freeagent_start_date = new DateTime(1900, 1, 1),
				first_accounting_year_end = new DateTime(1900, 1, 1),
				name = "Can't get company's name"
			};
		}

		public List<FreeAgentUsers> GetUsers(string accessToken) {
			var request = new RestRequest(Method.GET) { Resource = this.config.UsersRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);

			FreeAgentUsersList deserializedResponse = DeserializeResponse<FreeAgentUsersList>(response.Content);
			if (deserializedResponse != null && deserializedResponse.Users != null)
				return deserializedResponse.Users;

			log.Error("Failed parsing users. Request:{0} Response:{1}", request.Resource, response.Content);
			return new List<FreeAgentUsers>();
		}

		public FreeAgentExpensesList GetExpenses(string accessToken, DateTime? fromDate) {
			string fromDatePart = fromDate == null ? string.Empty : string.Format(this.config.ExpensesRequestDatePart, fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day);
			string expensesRequest = string.Format("{0}{1}", this.config.ExpensesRequest, fromDatePart);
			var request = new RestRequest(Method.GET) { Resource = expensesRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			var expenses = new List<FreeAgentExpense>();

			ExpensesListHelper deserializedResponse = DeserializeResponse<ExpensesListHelper>(response.Content);

			if (deserializedResponse != null && deserializedResponse.Expenses != null)
				expenses.AddRange(deserializedResponse.Expenses);
			else
				log.Error("Failed parsing expenses. Request:{0} Response:{1}", request.Resource, response.Content);

			string nextUrl = GetNextUrl(response);

			while (nextUrl != null) {
				request = new RestRequest(Method.GET) { Resource = nextUrl };
				request.AddHeader("Authorization", "Bearer " + accessToken);
				response = client.Execute(request);

				deserializedResponse = DeserializeResponse<ExpensesListHelper>(response.Content);

				if (deserializedResponse != null && deserializedResponse.Expenses != null)
					expenses.AddRange(deserializedResponse.Expenses);
				else
					log.Error("Failed parsing expenses. Request:{0} Response:{1}", request.Resource, response.Content);

				nextUrl = GetNextUrl(response);
			}

			var freeAgentExpenesList = new FreeAgentExpensesList(DateTime.UtcNow, expenses);
			return freeAgentExpenesList;
		}

		public FreeAgentExpenseCategory GetExpenseCategory(string accessToken, string categoryUrl) {
			var request = new RestRequest(Method.GET) { Resource = categoryUrl };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);

			ExpenseCategoriesListHelper deserializedResponse = DeserializeResponse<ExpenseCategoriesListHelper>(
				response.Content
			);

			if (deserializedResponse != null && deserializedResponse.Category != null)
				return deserializedResponse.Category;

			log.Error("Failed parsing category. Request:{0} Response:{1}", request.Resource, response.Content);
			return new FreeAgentExpenseCategory { url = string.Empty };
		}

		private string GetNextUrl(IRestResponse response) {
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
		} // GetNextUrl

		public AccessTokenContainer GetToken(string code, string redirectVal, out string errorMessage) {
			errorMessage = null;

			string getTokenRequestUrl = string.Format(
				"{0}?grant_type=authorization_code&code={1}&redirect_uri={2}&scope=&client_secret={3}&client_id={4}", this.config.OAuthTokenEndpoint,
				code,
				redirectVal, this.config.OAuthSecret, this.config.OAuthIdentifier
			);

			var request = (HttpWebRequest)WebRequest.Create(getTokenRequestUrl);

			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";

			log.Debug("Refresh token POST request URL: {0}", getTokenRequestUrl);

			try {
				using (var twitpicResponse = (HttpWebResponse)request.GetResponse()) {
					var response = twitpicResponse.GetResponseStream();

					if (response != null) {
						using (var reader = new StreamReader(response)) {
							string objText = reader.ReadToEnd();

							AccessTokenContainer deserializedResponse = (AccessTokenContainer)this.serializer.Deserialize(
								objText,
								typeof(AccessTokenContainer)
							);

							if ((deserializedResponse != null) && (deserializedResponse.access_token != null))
								return deserializedResponse;

							log.Error(
								"Failed to parse get access token response.\nRequest: {0}\nResponse: {1}",
								request.RequestUri,
								objText
							);

							throw new Exception(
								"Other error (parsing of get access token response did not throw any exception)."
							);
						} // using reader
					} // if response is not null
				} // using response
			} catch (Exception e) {
				errorMessage = "Failed to get an access token.";
				log.Warn(e, "{0}", errorMessage);
			} // try

			return null;
		} // GetToken

		public AccessTokenContainer RefreshToken(string refreshToken) {
			string refreshTokenRequestUrl = string.Format(
				"{0}?grant_type=refresh_token&refresh_token={1}&client_secret={2}&client_id={3}", this.config.OAuthTokenEndpoint,
				refreshToken, this.config.OAuthSecret, this.config.OAuthIdentifier
			);

			var request = (HttpWebRequest)WebRequest.Create(refreshTokenRequestUrl);

			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";

			log.Debug("Refresh token POST request URL: {0}", refreshTokenRequestUrl);

			try {
				using (var twitpicResponse = (HttpWebResponse)request.GetResponse()) {
					Stream response = twitpicResponse.GetResponseStream();

					if (response != null) {
						using (var reader = new StreamReader(response)) {
							string objText = reader.ReadToEnd();

							AccessTokenContainer deserializedResponse = (AccessTokenContainer)this.serializer.Deserialize(
								objText,
								typeof(AccessTokenContainer)
							);

							if ((deserializedResponse != null) && (deserializedResponse.access_token != null)) {
								deserializedResponse.refresh_token = refreshToken;
								return deserializedResponse;
							} // if

							log.Error(
								"Failed to refresh access token but parsing did not throw any exception.\n" +
								"Request: {0}\nResponse: {1}",
								request.RequestUri,
								objText
							);

							throw new Exception(
								"Other error (parsing of refresh access token response did not throw any exception)."
							);
						} // using reader
					} // if response is not null
				} // using response
			} catch (Exception e) {
				log.Warn(e, "Failed to refresh token.");
			} // try

			return null;
		} // RefreshToken

		public string GetApprovalRequest(string redirectUri) {
			string approvalRequestUrl = string.Format(
				"{0}?redirect_uri={1}&response_type=code&client_id={2}", this.config.OAuthAuthorizationEndpoint,
				redirectUri, this.config.OAuthIdentifier
			);

			log.Debug("Free Agent approval request URL: {0}", approvalRequestUrl);

			return approvalRequestUrl;
		} // GetApprovalRequest

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.client.Dispose();
		} // Dispose

		private T DeserializeResponse<T>(string responseContent) where T : class {
			try {
				return this.serializer.Deserialize<T>(responseContent);
			} catch (Exception e) {
				log.Error(
					e,
					"Failed to parse response from FreeAgent to type: {0}.\nResponse: {1}\n",
					typeof(T),
					responseContent
				);
				return null;
			} // try
		} // DeserializeResponse

		private readonly JavaScriptSerializer serializer;
		private readonly FreeAgentConfig config;
		private readonly HttpClient client;

		private static readonly ASafeLog log = new SafeILog(typeof(FreeAgentConnector));
	} // class FreeAgentConnector
} // namespace