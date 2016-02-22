using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobTest
{
	using System.Collections;
	using System.Collections.Specialized;
	using System.Net;
	using System.Security.Cryptography;
	using ConfigManager;
	using Ezbob.Utils;
	using Ezbob.Utils.Extensions;
	using NUnit.Framework;
	using RestSharp;

	[TestFixture]
	class Test_Ali {

		

		protected string urlPath = "param2/1/alibaba.open/partner.feedback/89978";
		protected string baseUrl = "http://119.38.217.38:1680/openapi/"; // "http://119.38.217.38:1680/openapi/";
		protected string appSecret = "IOVVt8lbOfDE";

		[Test]
		public void Test_Alibaba0001() {
			
			StringDictionary parameters = new StringDictionary();
			StringBuilder datas = new StringBuilder(this.urlPath);

			string json = "{\"bankCode\":\"ezbob\",\"bizType\":\"0001\",\"requestId\":\"000011\",\"aliMemberId\":\"3287122\",\"aId\":\"43623912\",\"loanId\":\"40748200\",\"requestedAmt\":100000.0,\"compEntityType\":\"LLC\",\"compName\":\"A PLAZA DRIVING SCHOOL\",\"compStreetAddr1\":\"926 E LEWELLING BLVD\",\"compCity\":\"HAYWARD\",\"compState\":\"CA\",\"compZip\":\"94541\",\"compPhone\":\"4152224444\",\"compEstablished\":\"2000-04-01\",\"compEmployees\":3,\"compRent\":180.0,\"compOtherLoans\":100.0,\"compOtherLeases\":120.0,\"compOwnershipPercent\":\"20-49%\",\"personalIncome\":499999.0,\"personalStreet1\":\"205 CHURCH ST\",\"personalCity\":\"VINE GROVE\",\"personalState\":\"KY\",\"personalZip\":\"40175\",\"personalPhone\":\"4151230987\",\"firstName\":\"MICHAEL\",\"lastName\":\"ARTE\",\"email\":\"ettester+sb-sesame304840@lendingclub.com\",\"compRevenue\":500000.0,\"compNetProfit\":100000.0,\"compCreLoans\":150.0,\"locOfferStatus\":\"App Submitted\",\"locOfferAmount\":83300.0,\"locOfferDate\":\"2015-02-09\",\"locOfferExpireDate\":\"2015-08-09\",\"locOfferCurrency\":\"GBP\",\"applicationDate\":\"2015-02-09\",\"countryId\":\"UK\"}";

			parameters.Add("data", json);

			var request = new RestRequest(this.urlPath, Method.POST);
			request.AddHeader("Content-type", "application/x-www-form-urlencoded; charset=UTF-8");

			// @TODO: add alphabetical sort for params

			// add json to POST form
			foreach (DictionaryEntry de in parameters) {
				request.AddParameter(de.Key.ToString(), de.Value);
				datas.Append(de.Key.ToString()).Append(de.Value);
			}

			// add alphabetical sort for params
			request.Parameters.ForEach(s => Console.WriteLine(s.Name + "=" + s.Value));

		//	Console.WriteLine(datas.ToString());

		//	Console.WriteLine(this.baseUrl);

			Encoding enc = System.Text.Encoding.UTF8;
			// from java: 3B819E81BAC9D0BC46808F58EE3FD64499FF16D1
			// .net		  3B819E81BAC9D0BC46808F58EE3FD64499FF16D1
			using (HMACSHA1 hmacsha1 = new HMACSHA1(enc.GetBytes(this.appSecret))) {
				byte[] digest = hmacsha1.ComputeHash(datas.ToString().ToStream());
				string signature = MiscUtils.ByteArr2Hex(digest).ToUpper();
				Console.WriteLine("_aop_signature: " + signature);
				request.AddParameter("_aop_signature", signature);
			}
			return;

			var client = new RestClient(this.baseUrl);
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
		public void TestAlibabaConnectionVars() {
			Console.WriteLine(CurrentValues.Instance.AlibabaMailTo.Value);
			//Console.WriteLine(CurrentValues.Instance.AlibabaBaseUrl_Sandbox.Value);
		//	Console.WriteLine(CurrentValues.Instance.AlibabaUrlPath_Sandbox.Value);
		}
		
	}
}
