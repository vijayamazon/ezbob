namespace Ezbob.API.AuthenticationAPI {
	using System;
	using System.ComponentModel.DataAnnotations;
	using System.Globalization;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using Ezbob.API.AuthenticationAPI.Models;
	using Newtonsoft.Json.Linq;
	using NUnit.Framework;
	using RestSharp;

	[TestFixture]
	public class Test_AuthorizedAPI {

		public class ClientCredentialModel {
			[Required]
			public string clientID { get; set; }
			[Required]
			public string clientSecret { get; set; }
			[Required]
			public string username { get; set; }
			[Required]
			public string userPwd { get; set; }
		}

		private string baseUrl = "https://localhost:44302/";
		private string authBaseUrl = "https://localhost:44302/";
		private string access_token = "";
		private AlibabaDto model;
		private JObject token;

		// grant_type=password&client_id=pAliServer7c60C021e70B&client_secret=152863423315581&username=partherAppAlibaba&password=XDroz4HhR1EI2zsvd
		private readonly ClientCredentialModel aliServerCredentials = new ClientCredentialModel {
			clientID = "pAliServer7c60C021e70B",
			clientSecret = "152863423315581",
			username = "partherAppAlibaba",
			userPwd = "XDroz4HhR1EI2zsvd"
		};

		//  grant_type=password&client_id=consoleApp&client_secret=123@abc=&username=elka&password=123456
		private readonly ClientCredentialModel testServerCredentials = new ClientCredentialModel {
			clientID = "consoleApp",
			clientSecret = "123@abc",
			username = "elka",
			userPwd = "123456"
		};

		// grant_type=password&client_id=pAliClient86f35Fd2896&username=partherAppAlibaba&password=XDroz4HhR1EI2zsvd
		// grant_type=password&client_id=ngAuthApp&username=elka&password=123456
		//	("ngAuthApp", "abc@123", "elka", "123456");
		private readonly ClientCredentialModel testClientCredentials = new ClientCredentialModel {
			clientID = "ngAuthApp",
			clientSecret = "abc@123",
			username = "elka",
			userPwd = "123456"
		};

		readonly IFormatProvider culture = Thread.CurrentThread.CurrentCulture;

		[TestFixtureSetUp]
		public void FixtureInit() {
			this.model = new AlibabaDto {
				requestId = "000001",
				responseId = "1000001",
				aId = 18234,
				aliMemberId = 12345,
				loanId = 0
			};
		}
		
		private void AuthRequest(string clientID, string clientSecret, string username, string userPwd) {

			/*Console.WriteLine(clientID);
			Console.WriteLine(clientSecret);
			Console.WriteLine(username);
			Console.WriteLine(userPwd);*/
			//return;


			if (this.token != null && this.token.GetValue(".expires") != null) {
				DateTime dt2 = DateTime.Parse(this.token.GetValue(".expires").ToString(), this.culture, DateTimeStyles.AssumeLocal);
				Console.WriteLine(dt2);
				TimeSpan timespan = (DateTime.Now - dt2);
				Console.WriteLine(timespan.TotalMinutes);
				if (timespan.TotalMinutes <= 30) {
					Console.WriteLine("token valid: " + this.token);
					return;
				}
			}

			RestClient client = new RestClient(this.authBaseUrl);

			RestRequest request = new RestRequest("token", Method.POST);
			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/x-www-form-urlencoded");
			request.AddParameter("grant_type", "password");
			request.AddParameter("client_id", clientID);
			request.AddParameter("client_secret", clientSecret);
			request.AddParameter("username", username);
			request.AddParameter("password", userPwd);

			try {
				var response = client.Post(request);

				Console.WriteLine(response.Content);

				if (response.StatusCode != HttpStatusCode.OK)
					throw new Exception(response.ErrorMessage);

				this.token = JObject.Parse(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}


		[Test]
		public void Test_Credit_Autorize_SSL() {
			try {
				AuthRequest(this.aliServerCredentials.clientID, this.aliServerCredentials.clientSecret, this.aliServerCredentials.username, this.aliServerCredentials.userPwd);
				Console.WriteLine(this.token);
			} catch (Exception e) {
				Console.WriteLine("Failed to get access token", e.Message);
				throw new HttpRequestException("Failed to get access token");
			}
			RestClient client = new RestClient(this.baseUrl);
			RestRequest request = new RestRequest("api/customers/availableCredit");
			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");
			request.AddHeader("Authorization", "Bearer " + this.token.GetValue("access_token"));
			request.AddJsonBody(this.model);
			try {
				var response = client.Post(request);
				Console.WriteLine(response.StatusCode);
				Console.WriteLine(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}


		private void callAvailCredit() {
			RestClient client = new RestClient(this.baseUrl);
			RestRequest request = new RestRequest("api/customers/availableCredit");
			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");
			request.AddHeader("Authorization", "Bearer " + this.token.GetValue("access_token"));
			request.AddJsonBody(this.model);
			try {
				var response = client.Post(request);
				Console.WriteLine(response.StatusCode);
				Console.WriteLine(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		[Test]
		public void Test_Cache() {

			try {
				AuthRequest(this.aliServerCredentials.clientID, this.aliServerCredentials.clientSecret, this.aliServerCredentials.username, this.aliServerCredentials.userPwd);
				Console.WriteLine(this.token);
			} catch (Exception e) {
				Console.WriteLine("Failed to get access token", e.Message);
				throw new HttpRequestException("Failed to get access token");
			}

			Console.WriteLine(string.Format("==={0},=========={1}======", this.model.aId, this.model.aliMemberId));
			this.callAvailCredit();
			this.callAvailCredit();
			//this.callAvailCredit();
			//this.callAvailCredit();

			this.model = new AlibabaDto {
				requestId = "000001",
				responseId = "1000001",
				aId = 16816,
				aliMemberId = 123,
				loanId = 0
			};
			Console.WriteLine(string.Format("==={0},=========={1}======", this.model.aId, this.model.aliMemberId));
			// other 
			this.callAvailCredit();
			this.callAvailCredit();
			//this.callAvailCredit();
			//this.callAvailCredit();
		}

		[Test]
		public void Test_Requalify_Autorize_SSL() {
			try {
				AuthRequest(this.aliServerCredentials.clientID, this.aliServerCredentials.clientSecret, this.aliServerCredentials.username, this.aliServerCredentials.userPwd);
				Console.WriteLine(this.token);
			} catch (Exception e) {
				Console.WriteLine("Failed to get access token", e.Message);
				throw new HttpRequestException("Failed to get access token");
			}
			//return;
			RestClient client = new RestClient(this.baseUrl);
			RestRequest request = new RestRequest("api/customers/qualify");
			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");
			request.AddHeader("Authorization", "Bearer " + this.token.GetValue("access_token"));
			request.AddJsonBody(this.model);
			try {
				var response = client.Post(request);
				Console.WriteLine(response.StatusCode);
				Console.WriteLine(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		[Test]
		public void Test_Credit_OtherUserAutorize_SSL() {
			try {
				AuthRequest(this.testServerCredentials.clientID, this.testServerCredentials.clientSecret, this.testServerCredentials.username, this.testServerCredentials.userPwd);
				Console.WriteLine(this.token);
			} catch (Exception e) {
				Console.WriteLine("Failed to get access token", e.Message);
				throw new HttpRequestException("Failed to get access token");
			}

			RestClient client = new RestClient(this.baseUrl);

			RestRequest request = new RestRequest("api/customers/availableCredit");
			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");
			request.AddHeader("Authorization", "Bearer " + this.token.GetValue("access_token"));
			request.AddJsonBody(this.model);
			try {
				var response = client.Post(request);
				Console.WriteLine(response.StatusCode);
				Console.WriteLine(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		[Test]
		public void Test_Credit_User_NotAutorized() {
			RestClient client = new RestClient(this.baseUrl);
			Console.WriteLine(this.baseUrl);
			RestRequest request = new RestRequest("api/customers/availableCredit");
			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");
			request.AddHeader("Authorization", "Bearer sadfsadf");
			request.AddJsonBody(this.model);
			try {
				var response = client.Post(request);
				Console.WriteLine(response.StatusCode);
				Console.WriteLine(response.Content);

			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		[Test]
		public void Test_SimpleGet() {
			RestRequest	request = new RestRequest("api/customers/testuser");
			RestClient	client = new RestClient(this.baseUrl);
			var response = client.Get(request);
			Console.WriteLine(response.Content);
		}

		/*[Test]
		public void hashed() {
			Console.WriteLine(Helper.GetHash("152863423315581"));
			//Console.WriteLine(Helper.GetHash("rM7jVn+pwmiEuKLCAjHaQzt+mmrKFBZqPH4nxHcwjOg="));
		}*/
		
		[Test]
		public void Test_Credit_ClentTypeJavascript() {
			try {
				AuthRequest(this.testClientCredentials.clientID, this.testClientCredentials.clientSecret, this.testClientCredentials.username, this.testClientCredentials.userPwd);
				Console.WriteLine(this.token);
			} catch (Exception e) {
				Console.WriteLine("Failed to get access token", e.Message);
				throw new HttpRequestException("Failed to get access token");
			}
			RestClient client = new RestClient(this.baseUrl);
			RestRequest request = new RestRequest("api/customers/availableCredit");
			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");
			request.AddHeader("Authorization", "Bearer " + this.token.GetValue("access_token"));
			request.AddJsonBody(this.model);
			try {
				var response = client.Post(request);
				Console.WriteLine(response.StatusCode);
				Console.WriteLine(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}


	}
}