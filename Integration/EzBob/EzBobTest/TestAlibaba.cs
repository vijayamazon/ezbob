
namespace EzBobTest {

	using NUnit.Framework;
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.IO;
	using System.Net;
	using System.Security.Cryptography;
	using System.Text;
	using System.Web.Script.Serialization;
	using Ezbob.Utils;
	using Ezbob.Utils.Extensions;
	using EzbobAPI;
	using log4net;
	using NHibernate.Linq;
	using RestSharp;
	
	[TestFixture]
	public class TestAlibaba {

		protected readonly ILog Log = LogManager.GetLogger(typeof(TestSalesForce));

		/*[SetUp]
		public new void Init() {
			log4net.Config.XmlConfigurator.Configure();
		}*/

		[Test]
		public void TestLog() {
			Log.Debug("This is a test");
		}

		//parameters.Add("data", "{ \"bankCode\": \"ezbob\",  \"bizType\": \"0001\", \"buyerAliId\": \"1004630888\", 	\"loanId\": \"lc9595958\"}");
		[Test]
		public void Test_AlibabaConnecitvity() {

			String urlPath = "param2/1/alibaba.open/partner.feedback/89978";
			String baseUrl = "http://119.38.217.38:1680/api/";
			String appSecret = "IOVVt8lbOfDE";

			StringDictionary parameters = new StringDictionary();
			StringBuilder datas = new StringBuilder(urlPath);

			parameters.Add("data", "{ \"bankCode\":\"ezbob\",\"bizType\":\"0001\"}"); //\"xxx\":\"yyy\",

			var request = new RestRequest(urlPath, Method.POST);
			request.AddHeader("Content-type", "application/x-www-form-urlencoded; charset=UTF-8");

			// add json to POST form
			foreach (DictionaryEntry de in parameters) {
				request.AddParameter(de.Key.ToString(), de.Value);
				datas.Append(de.Key.ToString()).Append(de.Value);
			}

			//	string datas = "param2/1/alibaba.open/partner.feedback/89978data{\"bankCode\":\"ezbob\",\"bizType\":\"0001\"}";

			Encoding enc = System.Text.Encoding.UTF8;

			using (HMACSHA1 hmacsha1 = new HMACSHA1(enc.GetBytes(appSecret))) {
				byte[] digest = hmacsha1.ComputeHash(datas.ToString().ToStream());
				string signature = MiscUtils.ByteArr2Hex(digest).ToUpper();
				Console.WriteLine(signature);
				request.AddParameter("_aop_signature", signature);
			}

			//	signature = "6EBE876BD0885923B2227C89B9F042CC7DA4FEDF";
			var client = new RestClient(baseUrl);
			try {
				IRestResponse response = client.Execute(request);
				HttpStatusCode status = response.StatusCode;

				Console.WriteLine(status);
				Console.WriteLine(response.Content);

			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}


		[Test]
		public void Test_StringDictionary() {
			string[] ar = { "zozobra", "kukka", "barabashka", "abrakadabra" };
			Array.Sort<string>(ar);
			ar.ForEach(x => Console.WriteLine(x));
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
		public void TestBasicAuthentication ( ) {
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

			/*
			CompositeType data = new CompositeType();
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
			}
			*/
		}

		[Test]
		public void Test_testauth() 
		{
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
		public void Test_AvailableCredit() {
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





	} // class TestMedal

} // namespace
