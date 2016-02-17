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
			return await RunService("CreateBrokerAccount", requestJson);
		}//CreateBrokerAccount

		public async Task<GetAccountByIDResponse> GetAccountByID(GetAccountByIDRequest requestModel) {
			var requestJson = JsonConvert.SerializeObject(requestModel);
			var response = await RunService("GetAccountByEmail", requestJson);
			if (response != null && !string.IsNullOrEmpty(response.message) && response.success)
				return JsonConvert.DeserializeObject<GetAccountByIDResponse>(response.message);
			return null;
		}//GetAccountByID

		public async Task<RestApiResponse> CreateUpdateLeadAccount(LeadAccountModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			return await RunService("EzbobWebServicesNew/LeadAccountService", requestJson);
		}//CreateUpdateLeadAccount

		public async Task<RestApiResponse> CreateOpportunity(OpportunityModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			return await RunService("EzbobWebServicesNew/CreateOpportunityService", requestJson);
		}//CreateOpportunity

		public async Task<RestApiResponse> UpdateOpportunity(OpportunityModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			return await RunService("EzbobWebServicesNew/UpdateCloseOpportunityService", requestJson);
		}//UpdateOpportunity

		public async Task<RestApiResponse> CreateUpdateContact(ContactModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			return await RunService("EzbobWebServicesNew/ContactService", requestJson);
		}//CreateUpdateContact

		public async Task<RestApiResponse> CreateTask(TaskModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			return await RunService("EzbobWebServicesNew/CreateTask", requestJson);
		}//CreateTask

		public async Task<RestApiResponse> CreateActivity(ActivityModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			return await RunService("EzbobWebServicesNew/CreateActivity", requestJson);
		}//CreateActivity

		public async Task<RestApiResponse> ChangeEmail(ChangeEmailModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			return await RunService("EzbobWebServicesNew/ChangeEmail", requestJson);
		}//ChangeEmail

		public async Task<GetActivityResultModel> GetActivity(GetActivityModel model) {
			var requestJson = JsonConvert.SerializeObject(model, Formatting.Indented);
			var response = await RunService("SearchLeadAndAccountByEmail", requestJson);
			GetActivityResultModel resultModel = new GetActivityResultModel { Error = response.errorCode };
			if (response.success) {
				resultModel.Activities = JsonConvert.DeserializeObject<IEnumerable<ActivityResultModel>>(response.message);
			}
			return resultModel;
		}//GetActivity

		private async Task<RestApiResponse> RunService(string service, string requestJson) {
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
				var restApiResponse = JsonConvert.DeserializeObject<RestApiResponse>(result);
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
