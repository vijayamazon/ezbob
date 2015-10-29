namespace FreeAgent {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using EzBob.CommonLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using Newtonsoft.Json;

	public class FreeAgentConnector : IDisposable {
		public const string NotFoundCompanyName = "Can't get company's name";

		public FreeAgentConnector() {
			this.expenseCategories = null;

			this.config = new FreeAgentConfig();

			this.client = new HttpClient { BaseAddress = new Uri(this.config.ApiBase), };
		} // constructor

		public FreeAgentInvoicesList GetInvoices(string accessToken, int numOfMonths) {
			string monthPart = numOfMonths == -1
				? string.Empty
				: string.Format(this.config.InvoicesRequestMonthPart, numOfMonths);

			var invoices = new List<FreeAgentInvoice>();

			FetchRemoteData<InvoicesListHelper>(
				"invoices",
				accessToken,
				this.config.InvoicesRequest,
				new [] { this.config.InvoicesRequestNestedPart, monthPart, },
				deserializedInvoiceList => invoices.AddRange(deserializedInvoiceList.Invoices)
			);

			var freeAgentInvoicesList = new FreeAgentInvoicesList(DateTime.UtcNow, invoices);

			log.Debug("Total {0} fetched from Free Agent.", Grammar.Number(freeAgentInvoicesList.Count, "invoice"));

			return freeAgentInvoicesList;
		} // GetInvoices

		public FreeAgentCompany GetCompany(string accessToken) {
			FreeAgentCompanyList company = null;

			FetchRemoteData<FreeAgentCompanyList>(
				"company",
				accessToken,
				this.config.CompanyRequest,
				null,
				deserializedCompanyList => { company = deserializedCompanyList; }
			);

			return company != null ? company.Company : new FreeAgentCompany {
				company_start_date = new DateTime(1900, 1, 1),
				freeagent_start_date = new DateTime(1900, 1, 1),
				first_accounting_year_end = new DateTime(1900, 1, 1),
				name = NotFoundCompanyName,
			};
		} // GetCompany

		public List<FreeAgentUsers> GetUsers(string accessToken) {
			FreeAgentUsersList users = null;

			FetchRemoteData<FreeAgentUsersList>(
				"users",
				accessToken,
				this.config.UsersRequest,
				null,
				deserializedUsersList => { users = deserializedUsersList; }
			);

			return (users != null) ? users.Users : new List<FreeAgentUsers>();
		} // GetUsers

		public FreeAgentExpensesList GetExpenses(string accessToken, DateTime? fromDate) {
			string fromDatePart = (fromDate == null) ? string.Empty : string.Format(
				this.config.ExpensesRequestDatePart,
				fromDate.Value.Year,
				fromDate.Value.Month,
				fromDate.Value.Day
			);

			var expenses = new List<FreeAgentExpense>();

			FetchRemoteData<ExpensesListHelper>(
				"expenses",
				accessToken,
				this.config.ExpensesRequest,
				new [] { fromDatePart, },
				deserializedExpenseList => expenses.AddRange(deserializedExpenseList.Expenses)
			);

			var freeAgentExpensesList = new FreeAgentExpensesList(DateTime.UtcNow, expenses);

			log.Debug("Total {0} fetched from Free Agent.", Grammar.Number(freeAgentExpensesList.Count, "expense"));

			return freeAgentExpensesList;
		} // GetExpenses

		public FreeAgentExpenseCategory GetExpenseCategory(string accessToken, string categoryUrl) {
			if (this.expenseCategories == null) {
				FetchRemoteData<ExpenseCategoriesListHelper>(
					"expense categories",
					accessToken,
					this.config.ExpensesCategoriesRequest,
					null,
					deserializedExpenseList => { this.expenseCategories = deserializedExpenseList; }
				);

				this.expenseCategories.UpdateSearchTree();
			} // if

			if (this.expenseCategories == null) {
				this.expenseCategories = new ExpenseCategoriesListHelper();
				this.expenseCategories.UpdateSearchTree();
			} // if

			FreeAgentExpenseCategory cat = this.expenseCategories[categoryUrl];

			log.Debug("Expense category '{0}' was mapped to '{1}'.", categoryUrl, cat.description);

			return cat;
		} // GetExpenseCategory

		public AccessTokenContainer GetToken(string code, string redirectVal, out string errorMessage) {
			errorMessage = null;

			string getTokenRequestUrl = string.Format(
				"{0}?grant_type=authorization_code&code={1}&redirect_uri={2}&scope=&client_secret={3}&client_id={4}",
				this.config.OAuthTokenEndpoint,
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

							AccessTokenContainer deserializedResponse = JsonConvert.DeserializeObject<AccessTokenContainer>(
								objText
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
				"{0}?grant_type=refresh_token&refresh_token={1}&client_secret={2}&client_id={3}",
				this.config.OAuthTokenEndpoint,
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

							AccessTokenContainer deserializedResponse = JsonConvert.DeserializeObject<AccessTokenContainer>(
								objText
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

		private T DeserializeResponse<T>(string responseContent) where T : class, IFreeAgentItemContainer {
			T result;

			try {
				result = JsonConvert.DeserializeObject<T>(responseContent);

				if (result == null)
					log.Warn("Parsing result is null.");
				else if (!result.HasItems()) {
					log.Warn("No items received.");
					result = null;
				} // if
			} catch (Exception e) {
				log.Error(
					e,
					"Failed to parse response from FreeAgent to type: {0}.\nResponse: {1}\n",
					typeof(T),
					responseContent
				);

				result = null;
			} // try

			return result;
		} // DeserializeResponse

		private void FetchRemoteData<TDeserialize>(
			string jobName,
			string accessToken,
			string path,
			IEnumerable<string> queryParts,
			Action<TDeserialize> handleDeserialized
		) where TDeserialize: class, IFreeAgentItemContainer {
			int pageNo = 0;
			bool hasNextPage;

			string queryBase = queryParts == null
				? null
				: string.Join("&", queryParts.Where(s => !string.IsNullOrWhiteSpace(s)));

			do {
				pageNo++;

				string pageArg = "page=" + pageNo;

				string query = string.IsNullOrWhiteSpace(queryBase)
					? pageArg
					: string.Format("{0}&{1}", queryBase, pageArg);

				string requestUri = string.Format("{0}?{1}", path, query);

				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
				request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
				request.Headers.UserAgent.Add(new ProductInfoHeaderValue("EzbobFreeAgentConnector", "2.0"));

				log.Debug(
					"Executing '{0}' page #{1} {2} request:\nURL: {3}\nHeaders:\n\t{4}",
					jobName,
					pageNo,
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
					"Page #{0} of {1} response status code is {2}, content:\n{3}",
					pageNo,
					jobName,
					response.StatusCode,
					responseContent
				);

				CheckForError(responseContent);

				TDeserialize deserializedResponse = DeserializeResponse<TDeserialize>(responseContent);

				if (deserializedResponse != null) {
					log.Debug(
						"{0} items fetched from page #{1} for {2}.",
						deserializedResponse.GetItemCount(),
						pageNo,
						jobName
					);
					handleDeserialized(deserializedResponse);
				} else
					log.Error("Failed to parse page #{0} data while fetching {1}.", pageNo, jobName);

				hasNextPage = false;

				foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers) {
					if (header.Key != "Link")
						continue;

					foreach (string headerValue in header.Value) {
						string[] links = headerValue.Split(',');

						foreach (string link in links) {
							if (!link.Contains("rel='next'"))
								continue;

							hasNextPage = true;
							break;
						} // for each link

						if (hasNextPage)
							break;
					} // for each

					if (hasNextPage)
						break;
				} // for each response header
			} while (hasNextPage);
		} // FetchRemoteData

		private void CheckForError(string responseContent) {
			if (string.IsNullOrWhiteSpace(responseContent))
				return;

			FreeAgentErrorList errorList;

			try {
				errorList = JsonConvert.DeserializeObject<FreeAgentErrorList>(responseContent);
			} catch (Exception e) {
				log.Debug(e, "Failed to deserialize FreeAgent output as an error object.");
				return;
			} // try

			if ((errorList == null) || (errorList.Error == null))
				return;

			if (errorList.Error.Details == null)
				throw new Exception("FreeAgent returned an error without details: " + responseContent);

			var os = new List<string>();

			foreach (KeyValuePair<string, string> error in errorList.Error.Details)
				os.Add(string.Format("{0}: {1}", error.Key, error.Value));

			if (os.Count < 1)
				throw new Exception("FreeAgent returned an error with empty list of details: " + responseContent);

			throw new Exception(string.Join("; ", os) + ".");
		} // CheckForError

		// Used implicitly during API response parsing.
		// ReSharper disable once ClassNeverInstantiated.Local
		// ReSharper disable once UnusedAutoPropertyAccessor.Local
		private class FreeAgentError {
			[JsonProperty(PropertyName = "error")]
			public Dictionary<string, string> Details { get; set; }
		} // class FreeAgentError

		// Used implicitly during API response parsing.
		// ReSharper disable once ClassNeverInstantiated.Local
		// ReSharper disable once UnusedAutoPropertyAccessor.Local
		private class FreeAgentErrorList {
			[JsonProperty(PropertyName = "errors")]
			public FreeAgentError Error { get; set; }
		} // class FreeAgentErrorList

		private readonly FreeAgentConfig config;
		private readonly HttpClient client;

		private ExpenseCategoriesListHelper expenseCategories;

		private static readonly ASafeLog log = new SafeILog(typeof(FreeAgentConnector));
	} // class FreeAgentConnector
} // namespace