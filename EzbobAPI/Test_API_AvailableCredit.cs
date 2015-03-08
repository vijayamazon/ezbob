namespace EzbobAPI {
	using System;
	using EzbobAPI.DataObject;
	using Newtonsoft.Json;
	using NUnit.Framework;
	using RestSharp;

	[TestFixture]
	public class Test_API_AvailableCredit {

		/** REST client usage:  POST to url: https://localhost:12498/Customer.svc/availableCredit
		Requeired headers:	  	 
Accept: application/json
Content-Type: application/json; charset=UTF-8
		--------------------
		Request Body: 	
		 * { "requestId": "000001", "responseId": "1000001", "aliMemberId" : "123", "loanId": "0", "aId" : "555" } // with errors
		 * 
		 * { "requestId": "000001", "responseId": "1000001", "aliMemberId" : "12345", "loanId": "0", "aId" : "18234" } // correct
		***/

		protected string customerBaseURL = "https://localhost:12498/Alibaba/Service.svc/";
		protected RestRequest request;
		protected AlibabaCompositeType input;
		private RestClient client;

		protected void InitRequest() {
			this.request = new RestRequest("availableCredit", Method.POST) { RequestFormat = DataFormat.Json };
			this.input = new AlibabaCompositeType();

			this.request.AddHeader("WWW-Authenticate", "test1:1tset");
			this.request.AddHeader("Accept", "application/json");
			this.request.AddHeader("Content-type", "application/json; charset=UTF-8");

			this.input.requestId = "000001";
			this.input.responseId = "1000001";
			this.input.aId = "18234"; //  "211"; // customerID
			this.input.aliMemberId = "12345"; // alibaba member ID
			this.input.loanId = "0";

			this.client = new RestClient(this.customerBaseURL);
		}

		[Test]
		public void WrongInputCustomerID() {
			InitRequest();
			this.input.aId = "asas";
			this.request.AddJsonBody(this.input);
			var response = this.client.Post(this.request);
			Console.WriteLine(response.Content);

			var obj = JsonConvert.DeserializeObject<AlibabaCompositeType>(response.Content);
			Assert.AreEqual(obj.errCode, AlibabaErrorCode.INCOMING_CUSTOMER_ID_NOT_VALID);
		}

		[Test]
		public void WrongInputAliMemberID() {
			InitRequest();
			this.input.aliMemberId = "asdfasdf32141243";
			this.request.AddJsonBody(this.input);
			var response = this.client.Post(this.request);
			Console.WriteLine(response.Content);

			var obj = JsonConvert.DeserializeObject<AlibabaCompositeType>(response.Content);
			Assert.AreEqual(obj.errCode, AlibabaErrorCode.INCOMING_ALI_MEMBER_ID_NOT_VALID);
		}

		[Test]
		public void WrongSystemDB_CustomerID() {
			InitRequest();
			this.input.aId = "0";
			this.request.AddJsonBody(this.input);
			var response = this.client.Post(this.request);
			Console.WriteLine(response.Content);

			var obj = JsonConvert.DeserializeObject<AlibabaCompositeType>(response.Content);
			Assert.AreEqual(obj.errCode, AlibabaErrorCode.SYSTEM_CUSTOMER_ID_NOT_FOUND);
		}

		[Test]
		public void WrongSystemDB_AliMemberID() {
			InitRequest();
			this.input.aliMemberId = "2141243";
			this.request.AddJsonBody(this.input);
			var response = this.client.Post(this.request);
			Console.WriteLine(response.Content);

			var obj = JsonConvert.DeserializeObject<AlibabaCompositeType>(response.Content);
			Assert.AreEqual(obj.errCode, AlibabaErrorCode.SYSTEM_ALI_MEMBER_ID_NOT_FOUND);
		}

		[Test]
		public void WrongSystemDB_MISMATCH() {
			InitRequest();
			this.input.aId = "18234";
			this.input.aliMemberId = "123";
			this.request.AddJsonBody(this.input);
			var response = this.client.Post(this.request);
			Console.WriteLine(response.Content);

			var obj = JsonConvert.DeserializeObject<AlibabaCompositeType>(response.Content);
			Assert.AreEqual(obj.errCode, AlibabaErrorCode.SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH);
		}

		[Test]
		public void Correct_NoError() {
			InitRequest();
			this.input.aId = "18234";
			this.input.aliMemberId = "12345";
			this.request.AddJsonBody(this.input);
			var response = this.client.Post(this.request);
			Console.WriteLine(response.Content);

			var obj = JsonConvert.DeserializeObject<AlibabaCompositeType>(response.Content);
			Assert.AreEqual(obj.errCode, AlibabaErrorCode.NO_ERROR);
		}

		[Test]
		public void No_REQUEST_ID() {
			InitRequest();
			this.input = new AlibabaCompositeType();
			this.request.AddJsonBody(this.input);
			var response = this.client.Post(this.request);
			Console.WriteLine(response.Content);

			var obj = JsonConvert.DeserializeObject<AlibabaCompositeType>(response.Content);
			Assert.AreEqual(obj.errCode, AlibabaErrorCode.INCOMING_REQUEST_ID_NOT_VALID);
		}

		[Test]
		public void No_RESPONSE_ID() {
			InitRequest();
			this.input = new AlibabaCompositeType();
			this.input.requestId = "3333333333333";
			this.request.AddJsonBody(this.input);
			var response = this.client.Post(this.request);
			Console.WriteLine(response.Content);

			var obj = JsonConvert.DeserializeObject<AlibabaCompositeType>(response.Content);
			Assert.AreEqual(obj.errCode, AlibabaErrorCode.INCOMING_RESPONSE_ID_NOT_VALID);
		}


		[Test]
		public void NO_VALID_CREDIT() {
			InitRequest();
			this.input.aId = "18234";
			this.input.aliMemberId = "12345";
			this.request.AddJsonBody(this.input);
			var response = this.client.Post(this.request);
			Console.WriteLine(response.Content);

			var obj = JsonConvert.DeserializeObject<AlibabaCompositeType>(response.Content);
		//	Assert.AreEqual(obj.errCode, AlibabaErrorCode.SYSTEM_NO_VALID_CREDITLINE_FOR_CUSTOMER);
		}

	}
}