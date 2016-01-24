namespace SalesForceLib {
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Threading.Tasks;
	using log4net;
	using Newtonsoft.Json;
	using SalesForceLib.Models;

	public class SalesForceService : ISalesForceService {
		public SalesForceService(
			string consumerKey, 
			string consumerSecret,
			string userName, 
			string password, 
			string token, 
			string environment) {
			this.consumerKey = consumerKey;
			this.consumerSecret = consumerSecret;
			this.userName = userName;
			this.password = password;
			this.token = token;
			this.environment = environment;
		}//ctor

		public async Task<LoginResultModel> Login() {
			string baseUrl;
			switch (this.environment) {
				case "Production":
					baseUrl = "https://login.salesforce.com";
					break;
				case "Sandbox":
					baseUrl = "https://test.salesforce.com";
					break;
				default:
					return null;
			}//switch

			var authClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
			//create login password value
			var loginPassword = this.password + this.token;
			HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string> {
				{"grant_type", "password"}, 
				{"client_id", this.consumerKey},
				{"client_secret", this.consumerSecret},
				{"username", this.userName}, 
				{"password", loginPassword}
			});

			var message = await authClient.PostAsync("/services/oauth2/token", content);
			var responseString = await message.Content.ReadAsStringAsync();
			var respose = JsonConvert.DeserializeObject<LoginResultModel>(responseString);

			Log.InfoFormat("The token value is {0} {1} {2}", responseString, respose.instance_url, respose.access_token);
			return respose;
		}//Login

		public async Task<RestApiResponse> CreateBrokerAccount(CreateBrokerRequest requestModel) {
			if (this.loginResult == null) {
				this.loginResult = await Login();
			}

			var httpClient = new HttpClient();
			httpClient.BaseAddress = new Uri(this.loginResult.instance_url);

			var requestJson = JsonConvert.SerializeObject(requestModel, Formatting.Indented);
			Log.InfoFormat("CreateBrokerAccount request json: {0}", requestJson);
			HttpContent content = new StringContent(requestJson);

			var request = new HttpRequestMessage(HttpMethod.Post, "/services/apexrest/CreateBrokerAccount");
			request.Content = content;
			request.Headers.Add("Authorization", string.Format("{0} {1}", this.loginResult.token_type, this.loginResult.access_token));
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			
			var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();
			var restApiResponse = JsonConvert.DeserializeObject<RestApiResponse>(result);
			Log.InfoFormat("CreateBrokerAccount response {0}", result);
			return restApiResponse;
		}//CreateBrokerAccount

		public async Task<GetAccountByIDResponse> GetAccountByID(GetAccountByIDRequest requestModel) {
			if (this.loginResult == null) {
				this.loginResult = await Login();
			}

			var httpClient = new HttpClient {
				BaseAddress = new Uri(this.loginResult.instance_url)
			};

			var requestJson = JsonConvert.SerializeObject(requestModel);
			Log.InfoFormat("GetAccountByID request json: {0}", requestJson);
			var request = new HttpRequestMessage(HttpMethod.Post, "/services/apexrest/GetAccountByEmail");
			request.Content = new StringContent(requestJson);
			request.Headers.Add("Authorization", string.Format("{0} {1}", this.loginResult.token_type, this.loginResult.access_token));
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();
			var response = JsonConvert.DeserializeObject<RestApiResponse>(result);
			Log.InfoFormat("GetAccountByID res {0}", result);
			if(response != null && !string.IsNullOrEmpty(response.message) && response.success)
				return JsonConvert.DeserializeObject < GetAccountByIDResponse>(response.message);
			return null;
		}//GetAccountByID

		private readonly string consumerKey;
		private readonly string consumerSecret;
		private readonly string userName;
		private readonly string password;
		private readonly string token;
		private readonly string environment;
		protected static readonly ILog Log = LogManager.GetLogger(typeof(SalesForceService));
		private LoginResultModel loginResult = null;
	}//SalesForceService
}//ns
