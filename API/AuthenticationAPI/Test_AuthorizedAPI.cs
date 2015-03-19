namespace Ezbob.API.AuthenticationAPI {
	using System;
	using System.Net;
	using System.Net.Http;
	using Ezbob.API.AuthenticationAPI.Models;
	using Newtonsoft.Json.Linq;
	using NUnit.Framework;
	using RestSharp;

	[TestFixture]
	public class Test_AuthorizedAPI {
		private string baseUrl = "https://localhost:44302/";
		private string authBaseUrl = "https://localhost:44302/";
		private AlibabaDto model;
		private JObject token;
	
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

				//Console.WriteLine(response.Content);

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
				AuthRequest("pAliServer7c60C021e70B", "152863423315581", "partherAppAlibaba", "XDroz4HhR1EI2zsvd");
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
		public void Test_Requalify_Autorize_SSL() {
			try {
				AuthRequest("pAliServer7c60C021e70B", "152863423315581", "partherAppAlibaba", "XDroz4HhR1EI2zsvd");
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
				AuthRequest("consoleApp", "123@abc", "elka", "123456");
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

	}
}