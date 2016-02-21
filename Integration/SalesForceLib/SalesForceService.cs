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
				case "Sb1":
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
			var requestJson = JsonConvert.SerializeObject(requestModel, Formatting.Indented);
			var response = await RunService<RestApiResponse>("CreateBrokerAccount", requestJson);
			RestApiResponse res = (RestApiResponse)response;
			return res;
		}//CreateBrokerAccount

		public async Task<GetAccountByIDResponse> GetAccountByID(GetAccountByIDRequest requestModel) {
			var requestJson = JsonConvert.SerializeObject(requestModel);
			var response = await RunService<RestApiResponse>("GetAccountByEmail", requestJson);
			var res = (RestApiResponse)response;
			if (response != null && !string.IsNullOrEmpty(res.message) && res.success)
				return JsonConvert.DeserializeObject<GetAccountByIDResponse>(res.message);
			return null;
		}//GetAccountByID

		public async Task<RestApiResponse> CreateUpdateLeadAccount(LeadAccountModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			var response = await RunService<RestApiResponse>("EzbobWebServicesNew/LeadAccountService", requestJson);
			RestApiResponse res = (RestApiResponse)response;
			return res;
		}//CreateUpdateLeadAccount

		public async Task<RestApiResponse> CreateOpportunity(OpportunityModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			var response = await RunService<RestApiResponse>("EzbobWebServicesNew/CreateOpportunityService", requestJson);
			RestApiResponse res = (RestApiResponse)response;
			return res;
		}//CreateOpportunity

		public async Task<RestApiResponse> UpdateOpportunity(OpportunityModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			var response = await RunService<RestApiResponse>("EzbobWebServicesNew/c", requestJson);
			RestApiResponse res = (RestApiResponse)response;
			return res;
		}//UpdateOpportunity

		public async Task<RestApiResponse> CreateUpdateContact(ContactModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			var response = await RunService<RestApiResponse>("EzbobWebServicesNew/ContactService", requestJson);
			RestApiResponse res = (RestApiResponse)response;
			return res;
		}//CreateUpdateContact

		public async Task<RestApiResponse> CreateTask(TaskModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			var response = await RunService<RestApiResponse>("EzbobWebServicesNew/CreateTask", requestJson);
			RestApiResponse res = (RestApiResponse)response;
			return res;
		}//CreateTask

		public async Task<RestApiResponse> CreateActivity(ActivityModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			var response = await RunService<RestApiResponse>("EzbobWebServicesNew/CreateActivity", requestJson);
			RestApiResponse res = (RestApiResponse)response;
			return res;
		}//CreateActivity

		public async Task<RestApiResponse> ChangeEmail(ChangeEmailModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			var resonse = await RunService<RestApiResponse>("EzbobWebServicesNew/ChangeEmail", requestJson);
			RestApiResponse res = (RestApiResponse)resonse;
			return res;
		}//ChangeEmail

		public async Task<GetActivityRestApiResonse> GetActivity(GetActivityModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			var response = await RunService<GetActivityRestApiResonse>("SearchLeadAndAccountByEmail", requestJson);
			GetActivityRestApiResonse res = (GetActivityRestApiResonse)response;
			return res;
		}//GetActivity

		private async Task<object> RunService<T>(string service, string requestJson) where T : RestApiResponse {
			if (this.loginResult == null) {
				this.loginResult = await Login();
			}

			var httpClient = new HttpClient();
			httpClient.BaseAddress = new Uri(this.loginResult.instance_url);

			Log.InfoFormat("{0} request json: {1}", service, requestJson);
			HttpContent content = new StringContent(requestJson);

			var request = new HttpRequestMessage(HttpMethod.Post, string.Format("/services/apexrest/{0}", service));
			request.Content = content;
			request.Headers.Add("Authorization", string.Format("{0} {1}", this.loginResult.token_type, this.loginResult.access_token));
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var response = await httpClient.SendAsync(request);
			var result = await response.Content.ReadAsStringAsync();
			Log.InfoFormat("{0} response {1}", service, result);
			try {
				var restApiResponse = JsonConvert.DeserializeObject<T>(result);
				return restApiResponse;
			} catch (Exception ex) {
				return new RestApiResponse {
					success = false,
					message = ex.Message,
					errorCode = "Failed Deserialize Response"
				};
			}
		}

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
