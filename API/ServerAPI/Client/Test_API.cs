namespace Ezbob.API.ServerAPI.Client {
	using System;
	using System.Net;
	using System.Net.Http;
	using Ezbob.API.ServerAPI.Models;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using NUnit.Framework;
	using RestSharp;

	[TestFixture]
	public class Test_API {

		private string baseUrl = "https://localhost:44301/";
		private string authBaseUrl = "https://localhost:44302/";

		private AlibabaDto model;
		private JObject token;

		private string client_id = "consoleApp";
		private string client_secret = "123@abc";
		//	private string app_username = "elka";
		//	private string app_password = "123456";
		private string app_username = "partherAppAlibaba" ; // "elka"
		private string app_password = "XDroz4HhR1EI2zsvd"; //123456  "XDroz4HhR1EI!2zsvd"

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

		/// <summary>
		/// grant_type=password&username=[username]&password=[pwd]&client_id=ngAuthApp
		/// grant_type=password&username=alibaba&password=123456&client_id=consoleApp&secret=123@abc
		/// polchasa
		/// </summary>
		/// <exception cref="Exception">Condition. </exception>
		[Test]
		public void Test_GetToken() {
			AuthenticateRequest(this.client_id, this.client_secret, this.app_username, this.app_password);
		}

		/*
		 * 	/*request.AddParameter("username", this.app_username);
			request.AddParameter("password", this.app_password);
			request.AddParameter("client_id", this.client_id); //"consoleApp"
			request.AddParameter("client_secret", this.client_secret); //"123@abc"*/
		private void AuthenticateRequest(string clientID, string clientSecret, string username, string userPwd) {

			RestClient client = new RestClient(this.authBaseUrl);
			RestRequest request = new RestRequest("token", Method.POST);

			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/x-www-form-urlencoded");

			request.AddParameter("grant_type", "password");
			
			request.AddParameter("client_id", clientID); //"consoleApp"
			request.AddParameter("client_secret", clientSecret); //"123@abc"
			request.AddParameter("username", username);
			request.AddParameter("password", userPwd);

			try {
				var response = client.Post(request);
				//Console.WriteLine(response.StatusCode + "==" + HttpStatusCode.OK);//Console.WriteLine(response.Content);
				if (response.StatusCode != HttpStatusCode.OK)
					throw new Exception(response.ErrorMessage);
				this.token = JObject.Parse(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}

		// POST: http://localhost:12359/api/customers/availableCredit		
		//	Accept: application/json
		//	Content-type:	application/json; charset=UTF-8
		//  Authorization: Bearer 2pIY6kx0EWK9Jqh2MfFEHaIXiG8-glF28PTrRpHS8ivvSb7Nhsw
		[Test]
		public async void Test_Credit_Protected() {
			try {
				Test_GetToken();
				Console.WriteLine(this.token.GetValue("access_token"));
			} catch (Exception) {
				Console.WriteLine("Failed to get access token");
				throw new HttpRequestException("Failed to get access token");
			}
			
			RestClient	client = new RestClient(this.baseUrl);
			RestRequest	request = new RestRequest("api/customers/availableCredit", Method.POST) { RequestFormat = DataFormat.Json };

			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");
			request.AddHeader("Authorization", "Bearer " + this.token.GetValue("access_token"));

			request.AddJsonBody(this.model);

			var response = client.Post(request);
			Console.WriteLine(response.Content);
		}

		/// <summary>
		/// 
		// POST	https://localhost:44301/
		//	Accept: application/json
		//	Content-type: application/x-www-form-urlencoded
		//  Authorization: Bearer 8l-X2orpwU0Ncq80Z9zdiOULheNEdplvDFCBttq0KkxM0Hjrm8kQv0oJdGolM2rZXGqS3eFmBGOwgto7sXlH78_bOSsCZlJ3rS0pLwq4LYG-YfL2bLbq0nNeYANKbqwQRDmmVX1JCkQtFHbDHJs7yUXXh3njQRTaEuWH67yn-yMqZKHe06r4NfL1BGk5G1PlPc0p2noYbI1SI9mkkxnB88Av0YOZpBhZsx4GK0m1XCJo1Bpkw18GnD0vwgdufPkC_b6rnaM2_4WsODE3mEm2pIY6kx0EWK9Jqh2MfFEHaIXiG8-glF28PTrRpHS8ivvSb7Nhsw
		/// </summary>
		[Test]
		public void Test_Autorized_Protected() {
			try {
				Test_GetToken();
				Console.WriteLine(this.token.GetValue("access_token"));
			} catch (Exception e) {
				Console.WriteLine("Failed to get access token", e.Message);
				throw new HttpRequestException("Failed to get access token");
			}
			RestClient client = new RestClient(this.baseUrl); 
			RestRequest request = new RestRequest("api/protected/protected");

			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");
			request.AddHeader("Authorization", "Bearer " + this.token.GetValue("access_token").ToString());

			request.AddJsonBody(this.model);
			var response = client.Get(request);

			Console.WriteLine();Console.WriteLine(response.Content);
		}


		/// <summary>
		// POST	https://localhost:44301/	
		//	Accept: application/json
		//	Content-type: application/x-www-form-urlencoded
		//  Authorization: Bearer 8l-X2orpwU0Ncq80Z9zdiOULheNEdplvDFCBttq0KkxM0Hjrm8kQv0oJdGolM2rZXGqS3eFmBGOwgto7sXlH78_bOSsCZlJ3rS0pLwq4LYG-YfL2bLbq0nNeYANKbqwQRDmmVX1JCkQtFHbDHJs7yUXXh3njQRTaEuWH67yn-yMqZKHe06r4NfL1BGk5G1PlPc0p2noYbI1SI9mkkxnB88Av0YOZpBhZsx4GK0m1XCJo1Bpkw18GnD0vwgdufPkC_b6rnaM2_4WsODE3mEm2pIY6kx0EWK9Jqh2MfFEHaIXiG8-glF28PTrRpHS8ivvSb7Nhsw
		/// </summary>
		[Test]
		public void Test_Credit_Autorize_SSL() {
			try {
		
		// app_username = ;  "elka"
		// app_password = ; //123456  "XDroz4HhR1EI!2zsvd"

				//AuthenticateRequest("consoleApp", "123@abc", "partherAppAlibaba", "XDroz4HhR1EI2zsvd");
				AuthenticateRequest("pAliServer7c60C021e70B", "123@abc", "partherAppAlibaba", "XDroz4HhR1EI2zsvd");

				//Console.WriteLine(this.token.GetValue("access_token"));
				Console.WriteLine(this.token);


			} catch (Exception e) {
				Console.WriteLine("Failed to get access token", e.Message);
				throw new HttpRequestException("Failed to get access token");
			}

			RestClient client = new RestClient(this.baseUrl);
			Console.WriteLine(this.baseUrl);

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
		
		//  var response = client.PostAsJsonAsync("api/employees/login", usrlogin).Result;

		[Test]
		public void Test_NonAutorized() {
			RestClient	client = new RestClient(this.baseUrl);
			RestRequest request = new RestRequest("api/protected");

			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");

			request.AddJsonBody(this.model);
			var response = client.Get(request);

			Console.WriteLine();
			Console.WriteLine(response.Content);
		}

		[Test]
		public void Test_SimpleGet() {
			RestRequest	request = new RestRequest("api/protected");
			RestClient	client = new RestClient(this.baseUrl);
			var response = client.Get(request);
			Console.WriteLine((response.Content));
		}


		[Test]
		public void Test_Get() {
			RestRequest	request = new RestRequest("api/customers/test/fff");
			RestClient	client = new RestClient(this.baseUrl);
			var response = client.Get(request);
			Console.WriteLine((response.Content));
		}

		[Test]
		public void Test_AvailCredit() {
			RestRequest	request = new RestRequest("api/customers/availableCredit", Method.POST) { RequestFormat = DataFormat.Json };
			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");
			RestClient	client = new RestClient(this.baseUrl);
			request.AddJsonBody(this.model);
			var response = client.Post(request);
			Console.WriteLine(JsonConvert.SerializeObject(response.Content));
		}


	}
}