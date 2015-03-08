namespace EzbobAPI {
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Net;
	using System.ServiceModel;
	using System.ServiceModel.Web;
	using System.Text;
	using System.Web.Script.Serialization;
	using EzbobAPI.DataObject;
	using Newtonsoft.Json;
	using NUnit.Framework;
	using RestSharp;

	[TestFixture]
	public class TestEzbobAPI {

		protected string customerBaseURL = "https://localhost:12498/Alibaba/Service.svc/"; 

		[Test]
		public void Test_WcfExt_BasicExample() {
			// Create binding with Transport security and Basic Authentication
			var binding = new WebHttpBinding(WebHttpSecurityMode.TransportCredentialOnly);
			binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

			// Create WebChannelFactory
			var factory = new WebChannelFactory<ICustomer>(binding, new Uri("https://localhost:12498/Service.svc/"));

			// Set client credential
			factory.Credentials.UserName.UserName = "username";
			factory.Credentials.UserName.Password = "password";

			// Create service proxy
			var proxy = factory.CreateChannel();
		}

		[Test]
		public void Test_CustomAuthorizationFromAllenConway() {
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(@"https://localhost:12498/Customer.svc/test1/1");

			//Add a header to the request that contains our credentials
			//DO NOT HARDCODE IN PRODUCTION!! Pull credentials real-time from database or other store.
			string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes("user1" + ":" + "test"));
			req.Headers.Add("Authorization", "Basic " + svcCredentials);

			//Just some example code to parse the JSON response using the JavaScriptSerializer
			using (WebResponse svcResponse = (HttpWebResponse)req.GetResponse()) {
				using (StreamReader sr = new StreamReader(svcResponse.GetResponseStream())) {
					JavaScriptSerializer js = new JavaScriptSerializer();
					string jsonTxt = sr.ReadToEnd();
					Console.WriteLine(jsonTxt);
				}
			}
		}

		[Test]
		public void Test_WcfExt_BasicWeb() {

			var request = new RestRequest("getdata/1", Method.GET);

			request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes("tom" + ":" + "123")));

			var client = new RestClient("https://localhost:12498/Customer.svc/");
			try {
				var response = client.Get(request);
				Console.WriteLine(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}


		[Test]
		public void Test_testauth() {
			var request = new RestRequest("testauth", Method.GET);

			request.AddHeader("WWW-Authenticate", "test1:1tset");
			request.AddHeader("Authorization", "a_test1:a_1tset");

			//request.AddHeader("Accept", "application/json");
			//request.AddHeader("Content-type", "application/json; charset=UTF-8");

			var client = new RestClient("https://localhost:12498/Customer.svc/");
			try {
				var response = client.Execute(request);
				Console.WriteLine(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}

		[Test]
		public void TestBasicAuth() {
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(@"https://localhost:12498/Customer/availableCredit/aaa@sss.com");
			//Add a header to the request that contains our credentials
			//DO NOT HARDCODE IN PRODUCTION!! Pull credentials real-time from database or other store.
			string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes("user1" + ":" + "test"));
			req.Headers.Add("Authorization", "Basic " + svcCredentials);
			//Just some example code to parse the JSON response using the JavaScriptSerializer
			using (WebResponse svcResponse = (HttpWebResponse)req.GetResponse()) {
				using (StreamReader sr = new StreamReader(svcResponse.GetResponseStream())) {
					JavaScriptSerializer js = new JavaScriptSerializer();
					string jsonTxt = sr.ReadToEnd();
				}
			}
		}


		[Test]
		public void Test_Signin() {
			var request = new RestRequest("login", Method.GET) { };
			request.AddQueryParameter("username", "testuser");
			request.AddQueryParameter("password", "Password01!");
			var client = new RestClient("https://localhost:12498/AuthenticationService.svc/");
			try {
				var response = client.Execute(request);
				Console.WriteLine("Content: {0} \n ContentLength: {1}, \n ResponseUri: {2}", response.Content, response.ContentLength, response.ResponseUri);
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}


		[Test]
		public void TestBasicAuthentication_1() {
			// Invoke authentication service to obtain the auth token.
			string uri = "http://localhost:12498/AuthenticationService.svc/Signin?username=testuser&password=Password01!";
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			if (response.StatusCode != HttpStatusCode.OK) {
				Console.WriteLine("Sign in failed.");
			}
			string authToken = null;
			using (Stream stream = response.GetResponseStream()) {
				using (StreamReader reader = new StreamReader(stream)) {
					authToken = reader.ReadToEnd();
					Console.WriteLine(authToken);
				}
			}
			// Invoke the real service.
			uri = "http://localhost:12489/RealService.svc/DoWork";
			request = request = (HttpWebRequest)HttpWebRequest.Create(uri);
			// Make sure to set the Authorization header.
			request.Headers["Authorization"] = authToken;
			response = (HttpWebResponse)request.GetResponse();
			if (response.StatusCode != HttpStatusCode.OK) {
				Console.WriteLine("Sign in failed.");
			}
			using (Stream stream = response.GetResponseStream()) {
				using (StreamReader reader = new StreamReader(stream)) {
					string result = reader.ReadToEnd();
					Console.WriteLine(result);
				}
			}
			Console.Read();
		}


		


		[Test]
		public void Test_APIpost() {
			/** REST client usage:  POST to url: https://localhost:12498/Customer.svc/qualify		
			Headers:	( MUST )		 
			Accept: application/json
			Content-Type: application/json; charset=UTF-8
			--------------------
			Request Body: 	{ "Email"  :  "asdfas@com",  "BoolValue": true, "StringValue" :  "adfasfdasf" }			 
			***/
			/*CompositeType data = new CompositeType();
			data.Email = "asdfas@com";
			data.BoolValue = false;
			var request = new RestRequest("qualify", Method.POST) {
				RequestFormat = DataFormat.Json
			};
			request.AddHeader("WWW-Authenticate", "test1:1tset");
			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");
			request.AddJsonBody(data);
			var client = new RestClient("https://localhost:12498/Customer.svc/");
			try {
				var response = client.Post(request);
				Console.WriteLine("Content: {0} \n ContentLength: {1}, \n ResponseUri: {2}", response.Content, response.ContentLength, response.ResponseUri);
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}*/
		}
		[Test]
		public void Test_AvailableCredit1() {
			string email = "aamirapa@gmail.com.test.test";
			var request = new RestRequest("availableCredit/aamirapa@gmail.com.test.test", Method.GET);

			/*request.AddHeader("WWW-Authenticate", "test1:1tset");
			request.AddHeader("Authorization", "a_test1:a_1tset");

			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");*/

			var client = new RestClient("https://localhost:12498/Customer.svc/");
			try {
				var response = client.Get(request);
				Console.WriteLine(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}
		[Test]
		public void Test_AvailableCredit() {

			/** REST client usage:  POST to url: https://localhost:12498/Customer.svc/availableCredit
			Headers:	( MUST )		 
Accept: application/json
Content-Type: application/json; charset=UTF-8
			--------------------
			Request Body: 	
			 * { "requestId": "000001", "responseId": "1000001", "aliMemberId" : "123", "loanId": "0", "aId" : "555" } // with errors
			 * 
			 * { "requestId": "000001", "responseId": "1000001", "aliMemberId" : "12345", "loanId": "0", "aId" : "18234" } // correct
			***/

			AlibabaCompositeType input = new AlibabaCompositeType();
			input.requestId = "000001";
			input.responseId = "1000001";
			input.aId = "18234"; //  "211"; // customerID
			input.aliMemberId = "12345"; // alibaba member ID
			input.loanId = "0";

			var request = new RestRequest("availableCredit", Method.POST) { RequestFormat = DataFormat.Json };

			request.AddHeader("WWW-Authenticate", "test1:1tset");
			request.AddHeader("Accept", "application/json");
			request.AddHeader("Content-type", "application/json; charset=UTF-8");
			request.AddJsonBody(input);

			var client = new RestClient(this.customerBaseURL);
			try {
				var response = client.Post(request);
				Debug.WriteLine(JsonConvert.SerializeObject(response.Content));
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}

		[Test]
		public void Test_AAA() {
			var request = new RestRequest("test1/1", Method.GET);
			request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes("user1" + ":" + "test")));
			var client = new RestClient("https://localhost:12498/Customer.svc/");
			try {
				var response = client.Get(request);
				Console.WriteLine(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}

	

	}
}