namespace SalesForceRestApiTestClient {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Text;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public class SalesForceService {
		public async Task<RestApiResponse> CreateBrokerAccount() {
			var res = await Login();
			
			HttpClient httpClient = new HttpClient();
			//httpClient.BaseAddress = new Uri("https://sb1-ezbob.cs87.force.com");
			httpClient.BaseAddress = new Uri(res.serviceUrl);
			
			//Create broker
			var createBrokerRequest = new CreateBrokerRequest {
				BrokerID = 115,
				ContactEmail = "alexbo+broker3@ezbob.com",
				Origin = "ezbob",
				ContactMobile = "01000000115",
				ContactName = "Another Good Broker",
				ContactOtherPhone = null,
				EstimatedMonthlyApplicationCount = 3,
				EstimatedMonthlyClientAmount = 1000,
				FCARegistered = false,
				FirmName = "Jada Coldfusion",
				FirmRegNum = "2340984",
				FirmWebSiteUrl = "http://www.ezbob.com",
				IsTest = true,
				LicenseNumber = null,
				SourceRef = "brk-assbx5"
			};


			string requestJson = JsonConvert.SerializeObject(createBrokerRequest, Formatting.Indented);
			Console.WriteLine(requestJson);
			HttpContent content = new StringContent(requestJson);

			var request = new HttpRequestMessage(HttpMethod.Post, "/services/apexrest/CreateBrokerAccount");
			request.Content = content;
			request.Headers.Add("Authorization", "Bearer " + res.oauthToken);
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			
			var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();
			var restApiResponse = JsonConvert.DeserializeObject<RestApiResponse>(result);
			Console.WriteLine("CreateBrokerAccount res {0}", JsonConvert.SerializeObject(restApiResponse));
			return restApiResponse;

		}

		public async Task<GetAccountByIDResponse> GetAccountByID(string email, string brand) {
			var res = await Login();

			HttpClient httpClient = new HttpClient();
			//httpClient.BaseAddress = new Uri("https://sb1-ezbob.cs87.force.com");
			httpClient.BaseAddress = new Uri(res.serviceUrl);

			//Get accountID
			var getAccountByIDRequest = new GetAccountByIDRequest {
				Email_in = "alexbo+broker3ezbob.com",
				Brand = "ezbob"
			};

			HttpContent content2 = new StringContent(JsonConvert.SerializeObject(getAccountByIDRequest), Encoding.UTF8, "application/json");
			var result2 = await httpClient.PostAsync("/rest/services/apexrest/GetAccountByEmail", content2);
			var result2Content = await result2.Content.ReadAsStringAsync();
			var getAccountByIDResponse = JsonConvert.DeserializeObject<GetAccountByIDResponse>(result2Content);
			Console.WriteLine("GetAccountByID res {0}", JsonConvert.SerializeObject(getAccountByIDResponse));
			return getAccountByIDResponse;
		}

		private async Task<LoginResult> Login() {
			//defined remote access app - develop --> remote access --> new
			//set OAuth key and secret variables
			string sfdcConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
			string sfdcConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
			//set to Force.com user account that has API access enabled
			string sfdcUserName = ConfigurationManager.AppSettings["ForceUserName"];
			string sfdcPassword = ConfigurationManager.AppSettings["ForcePassword"];
			string sfdcToken = ConfigurationManager.AppSettings["ForceToken"];

			string environment = ConfigurationManager.AppSettings["Environment"];
			string baseUrl = "";
			switch (environment) {
				case "Prod":
					baseUrl = "https://login.salesforce.com";
					break;
				case "Sandbox":
					baseUrl = "https://test.salesforce.com";
					break;
				default:
					return null;
			}

			HttpClient authClient = new HttpClient() { BaseAddress = new Uri(baseUrl) };
			//create login password value
			string loginPassword = sfdcPassword + sfdcToken;
			HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string> {
				{"grant_type", "password"}, 
				{"client_id", sfdcConsumerKey},
				{"client_secret", sfdcConsumerSecret},
				{"username", sfdcUserName}, 
				{"password", loginPassword}
			});

			HttpResponseMessage message = await authClient.PostAsync("/services/oauth2/token", content);
			string responseString = await message.Content.ReadAsStringAsync();
			JObject obj = JObject.Parse(responseString);
			LoginResult res = new LoginResult {
				oauthToken = (string)obj["access_token"],
				serviceUrl = (string)obj["instance_url"]
			};
			
			//print response values
			Console.WriteLine(string.Format("The token value is {0}", responseString));
			Console.WriteLine(string.Format("URL is  {0}", res.serviceUrl));
			Console.WriteLine(string.Format("OauthToken is  {0}", res.oauthToken));
			Console.WriteLine("");
			return res;
		}
	}

	public class LoginResult {
		public string oauthToken { get; set; }
		public string serviceUrl { get; set; }
	}
}
