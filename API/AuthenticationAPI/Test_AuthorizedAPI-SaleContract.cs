namespace Ezbob.API.AuthenticationAPI {
	using System;
	using System.ComponentModel.DataAnnotations;
	using System.Globalization;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using Ezbob.API.AuthenticationAPI.Models;
	using Ezbob.Backend.Models.ExternalAPI;
	using Newtonsoft.Json.Linq;
	using NUnit.Framework;
	using RestSharp;

	[TestFixture]
	public class Test_AuthorizedAPI_SaleContract {

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
		private AlibabaContractDto model;
		private JObject token;

		// grant_type=password&client_id=pAliServer7c60C021e70B&client_secret=152863423315581&username=partherAppAlibaba&password=XDroz4HhR1EI2zsvd
		private readonly ClientCredentialModel aliServerCredentials = new ClientCredentialModel {
			clientID = "pAliServer7c60C021e70B",
			clientSecret = "152863423315581",
			username = "partherAppAlibaba",
			userPwd = "XDroz4HhR1EI2zsvd"
		};

		// grant_type=password&client_id=pAliServerStaging8256&client_secret=8674067HBI90&username=partherAppAlibaba&password=086b469t
		private readonly ClientCredentialModel staging_aliServerCredentials = new ClientCredentialModel {
			clientID = "pAliServerStaging8256",
			clientSecret = "8674067HBI90",
			username = "partherAppAlibabaStaging",
			userPwd = "086b469t"
		};

		// grant_type=password&client_id=pAliServerStaging8256&client_secret=8674067HBI90&username=partherAppAlibaba&password=086b469t
		private readonly ClientCredentialModel staging_aliClentCredentials = new ClientCredentialModel {
			clientID = "pAliClientStaging8256",
			clientSecret = "9603JT6789S",
			username = "partherAppAlibabaStaging",
			userPwd = "086b469t"
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

		// { "requestId": "000771", "responseId": "000771", "aliMemberId" : 789, "loanId": "0", "aId" : 23504 }
		[TestFixtureSetUp]
		public void FixtureInit() {
			this.model = new AlibabaContractDto {
                requestId = "142857",
                responseId = "543",
                loanId = 24314474,
                orderNumber = "23456780",
                sellerBusinessName = "Dabao Trading Ltd.",
                sellerAliMemberId = "41523",
                sellerStreet1 = "128 Xihu Road",
                sellerCity = "Hangzhou",
                sellerState = "Zhejiang",
                sellerCountry = "China",
                sellerPostalCode = "310016",
                sellerAuthRepFname = "Hong",
                sellerAuthRepLname = "Zhang",
                sellerPhone = "865218526",
                sellerFax = "865218526",
                sellerEmail = "zhang.hong@163.com",
                buyerBusinessName = "A PLAZA DRIVING SCHOOL",
                aliMemberId = 131,
                aId = 15123,
                buyerStreet1 = "926 E LEWELLING BLVD",
                buyerCity = "HAYWARD",
                buyerState = "CA",
                buyerCountry = "U.S.A",
                buyerZip = "94541",
                buyerAuthRepFname = "MICHAEL",
                buyerAuthRepLname = "ARTE",
                buyerPhone = "415222444",
                buyerEmail = "marte@aplaza.net",
                shippingMark = "WT12345678",
                totalOrderAmount = 88000,
                deviationQuantityAllowed = 20,
                orderAddtlDetails = "",
                shippingTerms = "asd",
                shippingDate = new DateTime(2015,03,01),
                loadingPort = "Shanghai",
                destinationPort = "Oakland,CA",
                orderDeposit = 1000,
                beneficiaryBank = "Bank of China",
                bankAccountNumber = 1234567890,
                bankStreetAddr1 = "108 Ganjiang Road",
                bankCity = "Hangzhou",
                bankState = "Zhejiang",
                bankCountry = "China",
                bankPostalCode = "310020",
                swiftcode = "ADBNCNBJCD1",
                orderCurrency = "gbp",
                orderItems = new OrderItems[] {
                    new OrderItems(){orderProdNumber = 0105, productName = "Battery", productSpecs = "AAA", productQuantity = 20000, productUnit = 6, productUnitPrice = 2, productTotalAmount = 48000},
                    new OrderItems(){orderProdNumber = 0106, productName = "Screw", productSpecs = "Phillips", productQuantity = 50000, productUnit = 100, productUnitPrice = 8, productTotalAmount = 40000}
                }
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

				//Console.WriteLine(response.Content);

				if (response.StatusCode != HttpStatusCode.OK)
					throw new Exception(response.ErrorMessage);

				this.token = JObject.Parse(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}


		[Test]
		public void Test_Sale_Contract() {
			try {
                AuthRequest(this.testServerCredentials.clientID, this.testServerCredentials.clientSecret, this.testServerCredentials.username, this.testServerCredentials.userPwd);
				Console.WriteLine(this.token);
			} catch (Exception e) {
				Console.WriteLine("Failed to get access token", e.Message);
				throw new HttpRequestException("Failed to get access token");
			}
			RestClient client = new RestClient(this.baseUrl);
            RestRequest request = new RestRequest("api/Contracts/Contract");
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

			//Console.WriteLine(string.Format("==={0},=========={1}======", this.model.aId, this.model.aliMemberId));
			this.callAvailCredit();
			this.callAvailCredit();
			//this.callAvailCredit();
			//this.callAvailCredit();

            //this.model = new AlibabaDto {
            //    requestId = "000001",
            //    responseId = "1000001",
            //    aId = 23504,
            //    aliMemberId = 789,
            //    loanId = 0
            //};
			//Console.WriteLine(string.Format("==={0},=========={1}======", this.model.aId, this.model.aliMemberId));
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


		[Test]
		public void Ali_Staging_User_Available_Credit() {
			try {
			//	AuthRequest(this.staging_aliServerCredentials.clientID, this.staging_aliServerCredentials.clientSecret, this.staging_aliServerCredentials.username, this.staging_aliServerCredentials.userPwd);
				AuthRequest(this.staging_aliClentCredentials.clientID, this.staging_aliClentCredentials.clientSecret, this.staging_aliClentCredentials.username, this.staging_aliClentCredentials.userPwd);
				Console.WriteLine(this.token);
				//return;
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