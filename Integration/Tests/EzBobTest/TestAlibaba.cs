
namespace EzBobTest {
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Net;
	using System.Security.Cryptography;
	using System.Text;
	using ConfigManager;
	using Ezbob.Utils;
	using Ezbob.Utils.Extensions;
	using log4net;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using NUnit.Framework;
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
			//var c = new AlibabaClient();
			var c = CurrentValues.Instance;
			Console.WriteLine(c.AlibabaAppSecret_Sandbox.Value);
		}

		//parameters.Add("data", "{ \"bankCode\": \"ezbob\",  \"bizType\": \"0001\", \"buyerAliId\": \"alitest1\", 	\"loanId\": \"lc9595958\"}");
		[Test]
		public void Test_AlibabaConnecitvity() {

			String urlPath = "param2/1/alibaba.open/partner.feedback/89978";
			String baseUrl = "http://119.38.217.38:1680/api/";
			String appSecret = "IOVVt8lbOfDE";

			StringDictionary parameters = new StringDictionary();
			StringBuilder datas = new StringBuilder(urlPath);

			parameters.Add("data", "{\"bankCode\":\"ezbob\",\"bizType\":\"0001\"}");
			var request = new RestRequest(urlPath, Method.POST);

			request.AddHeader("Content-type", "application/x-www-form-urlencoded; charset=UTF-8");

			// add json to POST form
			foreach (DictionaryEntry de in parameters) {
				request.AddParameter(de.Key.ToString(), de.Value); // == parameters.Add("data", "{\"bankCode\":\"ezbob\",\"bizType\":\"0001\"}"); 
				datas.Append(de.Key.ToString()).Append(de.Value); // data{\"bankCode\":\"ezbob\",\"bizType\":\"0001\"}
			}
			// datas == {param2/1/alibaba.open/partner.feedback/89978data{"bankCode":"ezbob","bizType":"0001"}}
			//	string datas = "param2/1/alibaba.open/partner.feedback/89978data{\"bankCode\":\"ezbob\",\"bizType\":\"0001\"}";
			Encoding enc = Encoding.UTF8;
			using (HMACSHA1 hmacsha1 = new HMACSHA1(enc.GetBytes(appSecret))) {
				byte[] digest = hmacsha1.ComputeHash(datas.ToString().ToStream());
				string signature = MiscUtils.ByteArr2Hex(digest).ToUpper();
				Console.WriteLine(signature);
				request.AddParameter("_aop_signature", signature);
			}

			request.Parameters.ForEach( p => Console.WriteLine(p.Name + "=" + p.Value));

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


		[Test] // WORKING!!!
		public void Test_Alibaba0001() {
			//http://119.38.217.38:1680/openapi/param2/1/alibaba.open/partner.feedback/{appKey}
			String urlPath = "param2/1/alibaba.open/partner.feedback/89978";
			String baseUrl = "http://119.38.217.38:1680/openapi/";
			String appSecret = "IOVVt8lbOfDE";

			StringDictionary parameters = new StringDictionary();
			StringBuilder datas = new StringBuilder(urlPath);
			
			//string json = "{\"bankCode\": \"ezbob\",\"bizType\": \"0001\",\"requestID\": \"qa345we\",\"aliMemberId\": \"7776789\",\"compEntityType\":\"LLC\", \"compName\":\"blabla\", \"aId\": \"18234\",\"requestedAmt\": 30000.00,\"countryId\": \"UK\","
			//	+ "\"locOfferStatus\":\"Incomplete\", \"firstName\":\"Peter\",	\"lastName\": \"Joe\", \"personalPhone\": \"6176107849\" , \"email\": \"pjoejoejoe@outlook.com\", \"loanId\": \"0\" }";

			string	json="{\"countryId\":\"UK\",\"locOfferCurrency\":\"GBP\",\"aId\":18240,\"aliMemberId\":18240,\"loanId\":null,\"requestedAmt\":5000.0,\"compName\":\"POSITIVE NOISE LTD\",\"compStreetAddr1\":\"42 New Road\",\"compStreetAddr2\":\"\",\"compCity\":\"Rochester\",\"compState\":\"Kent\",\"compZip\":\"ME1 1DX\",\"compPhone\":\"00000000000\",\"compEstablished\":null,\"compEstablishedYears\":\"\",\"compEmployees\":0,\"compEntityType\":\"Limited\",\"compType\":\"\",\"firstName\":\"Martin\",\"lastName\":\"Martin  Young\",\"personalStreet1\":\"23 Priestfields\",\"personalStreet2\":\"\",\"personalCity\":\"Rochester\",\"personalState\":\"Kent\",\"personalZip\":\"ME1 3AB\",\"personalPhone\":\"00000000000\",\"personalphoneAlt\":\"00000000000\",\"email\":\"martin@positivenoise.com.test.test\",\"compRevenue\":630000.0,\"applicationDate\":\"2012-08-27T16:37:06Z\",\"locOfferStatus\":null,\"compRegId\":\"\",\"compDba\":\"\",\"website\":\"\",\"gender\":\"\",\"remarks\":\"\",\"rejectReason\":\"\",\"bankCode\":\"ezbob\",\"requestID\":18158565690,\"bizType\":\"0002\"}";

			parameters.Add("data", json);

			var request = new RestRequest(urlPath, Method.POST);
			request.AddHeader("Content-type", "application/x-www-form-urlencoded; charset=UTF-8");

			// add json to POST form
			foreach (DictionaryEntry de in parameters) {
				request.AddParameter(de.Key.ToString(), de.Value); 
				datas.Append(de.Key.ToString()).Append(de.Value); 
			}
			// add alphabetical sort for params
			request.Parameters.ForEach(s => Console.WriteLine(s.Name + "=" + s.Value));

			Console.WriteLine(datas.ToString());
			Console.WriteLine(baseUrl);

			Encoding enc = Encoding.UTF8;
			using (HMACSHA1 hmacsha1 = new HMACSHA1(enc.GetBytes(appSecret))) {
				byte[] digest = hmacsha1.ComputeHash(datas.ToString().ToStream());
				string signature = MiscUtils.ByteArr2Hex(digest).ToUpper();
				Console.WriteLine("_aop_signature: "+signature);
				request.AddParameter("_aop_signature", signature);
			}
			var client = new RestClient(baseUrl);
			try {
				IRestResponse response = client.Post(request);
				HttpStatusCode status = response.StatusCode;
				Console.WriteLine(status);
				Console.WriteLine(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		[Test]
		public void Test_Alibaba0001_1() {
			//http://119.38.217.38:1680/openapi/param2/1/alibaba.open/partner.feedback/{appKey}
			String urlPath = "param2/1/alibaba.open/partner.feedback/89978";
			String baseUrl = "http://119.38.217.38:1680/openapi/";
			String appSecret = "IOVVt8lbOfDE";

			string param1 = urlPath + "data{";
			StringBuilder datas = new StringBuilder(param1);

			var request = new RestRequest(urlPath, Method.POST);
			request.AddHeader("Content-type", "application/x-www-form-urlencoded; charset=UTF-8");

			/*string json = @"{
	bankCode: ""ezbob"" ,
	""bizType"": ""0001"",
	""requestID"": ""123abcd"",
	""aliMemberId"": ""123456789"", 
	""aId"": ""18234"",
	""loanId"": ""12345678"",
	""requestedAmt"": 30000.00, 
	""compName"": ""PJ Coffee Company"",
	""compStreetAddr1"": ""880 Boylston St."",
	""compStreetAddr2"": """", 
	""compCity"": ""Boston"", 
	""compState"": ""MA"", 
	""compZip"": ""02199"", 
	""compPhone"": ""6176107849"",
	""compEstablished"": ""2010-01-01"",
	""compEstablishedYears"": ""3"",
	""compEmployees"": 5, 
	""compEntityType"": ""SOLE_PROPRIETOR"",
	""firstName"": ""Peter"",
	""lastName"": ""Joe"",
	""personalStreet1"": ""880 Boylston St."",
	""personalStreet2"": """",
	""personalCity"": ""Boston"",
	""personalState"": ""MA"",
	""personalZip"": ""02199"",
	""personalPhone"": ""6176107849"", 
	""personalPhoneAlt"": """", 
	""email"": ""pjoejoejoe@outlook.com"", 
	""compRevenue"": 100000.00, 
	""compNetProfit"": 30000.00, 
	""compCreLoans"": 0, 
	""compRent"": 4000.00, 
	""compOtherLoans"": 0, 
	""compOtherLeases"": 0, 
	""personalIncome"": 80000.00,
	""compOwnershipPercent"": ""50-80"",
	""locApproveStatus"": ""No loan"",
	""compType"":""bike shop"",
	""applicationDate"": ""2014-12-23 12:00:00 US PT"",
	""locOfferStatus"":""Incomplete"",
	""locOfferAmount"":""0"",
	""locOfferCurrency"":""GBP"",
	""locOfferDate"":""Incomplete"",
	""locOfferExpireDate"":""Incomplete"" 
}";*/

			/*	status 200
	name=data, value={"bankCode": "ezbob","bizType": "0001","requestID": "123abcd","aliMemberId": "123456789",	"aId": "18234","requestedAmt": 30000.00,"countryId": "UK","locOfferStatus":"Incomplete"}
	datas: data{"bankCode": "ezbob","bizType": "0001","requestID": "123abcd","aliMemberId": "123456789","aId": "18234","requestedAmt": 30000.00,"countryId": "UK","locOfferStatus":"Incomplete"}
	datas finally: [param2/1/alibaba.open/partner.feedback/89978, data{"bankCode": "ezbob","bizType": "0001","requestID": "123abcd","aliMemberId": "123456789",	"aId": "18234","requestedAmt": 30000.00,"countryId": "UK","locOfferStatus":"Incomplete"}]
	name=_aop_signature, value=F841240BCF4D7AF6F80C0EA7C31436D2B593DD08
		*/

			string json = "{\"bankCode\": \"ezbob\",\"bizType\": \"0001\",\"requestID\": \"123abcd\",\"aliMemberId\": \"123456789\",\"aId\": \"18234\",\"requestedAmt\": 30000.00,\"countryId\": \"UK\",\"locOfferStatus\":\"Incomplete\"}";

			var jsonEntries = JObject.Parse(json).Properties().OrderBy(c => c.Name).ToList();

			int lastCount = jsonEntries.Count;
			foreach (var l in jsonEntries) {
				lastCount--;
				datas.Append(l);
				if (lastCount > 0)
					datas.Append(",");
				else
					datas.Append("}");
			}

			Console.WriteLine(datas.ToString());

			string finaljsonpost = JsonConvert.SerializeObject(datas.ToString().Remove(0, param1.Length - 1));

			Console.WriteLine(finaljsonpost);

			// add json to POST form
			request.AddParameter("data", finaljsonpost);

			/*
 param2/1/alibaba.open/partner.feedback/89978data{"aId": "18234","aliMemberId": "123456789","bankCode": "ezbob","bizType": "0001","countryId": "UK","locOfferStatus": "Incomplete","requestedAmt": 30000.0,"requestID": "123abcd"}
"{\"aId\": \"18234\",\"aliMemberId\": \"123456789\",\"bankCode\": \"ezbob\",\"bizType\": \"0001\",\"countryId\": \"UK\",\"locOfferStatus\": \"Incomplete\",\"requestedAmt\": 30000.0,\"requestID\": \"123abcd\"}"
8F02469CA5FC737D70C78661CF1CDF440E797CBF
				*/

			Encoding enc = Encoding.UTF8;

			using (HMACSHA1 hmacsha1 = new HMACSHA1(enc.GetBytes(appSecret))) {
				byte[] digest = hmacsha1.ComputeHash(datas.ToString().ToStream());
				string signature = MiscUtils.ByteArr2Hex(digest).ToUpper();
				Console.WriteLine(signature);
				request.AddParameter("_aop_signature", signature);
			}

			var client = new RestClient(baseUrl);
			try {
				IRestResponse response = client.Post(request);
				HttpStatusCode status = response.StatusCode;
				Console.WriteLine("====" + status);
				Console.WriteLine(response.Content);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}


		

		
		



	

	

	} // class TestMedal

} // namespace
