namespace EzBobTest {
	using System;
	using System.Net;
	using System.Text;
	using Newtonsoft.Json;
	using NUnit.Framework;
	using Twilio;

	[TestFixture]
	internal class TestTwilio {
		private TwilioRestClient twilioClient;
		private string fromPhone;

		/*
		 test phone numbers
		 
		 From
				Your test credentials don't have access to any valid 'From' phone numbers on your real account. Therefore the only phone numbers you should use as 'From' numbers are the magic numbers listed here.

				VALUE	DESCRIPTION	ERROR CODE
				+15005550001	This phone number is invalid.	21212
				+15005550007	This phone number is not owned by your account or is not SMS-capable.	21606
				+15005550008	This number has an SMS message queue that is full.	21611
				+15005550006	This number passes all validation.	No error
				All Others	This phone number is not owned by your account or is not SMS-capable.	21606
		To

				VALUE	DESCRIPTION	ERROR CODE
				+15005550001	This phone number is invalid.	21211
				+15005550002	Twilio cannot route to this number.	21612
				+15005550003	Your account doesn't have the international permissions necessary to SMS this number.	21408
				+15005550004	This number is blacklisted for your account.	21610
				+15005550009	This number is incapable of receiving SMS messages.	21614
				All Others	Any other phone number is validated normally.	Input-dependent

		 */


		[SetUp]
		public void Init() {
			InitProd();
		//	InitSandbox();
		}

		public void InitProd(){
			//prod
			this.twilioClient = new TwilioRestClient("ACcc682df6341371ee27ada6858025490b", "fab0b8bd342443ff44497273b4ba2aa1");
			this.fromPhone = "+441301272000"; //UK only phone number
			//this.fromPhone = "+972526285470"; //Israel only phone number
		}

		public void InitSandbox() {
			//sandbox
			this.twilioClient = new TwilioRestClient("AC763a10874713c9d2f502aad30417073f", "29b8830923bbc679f8a501851916e3b8");
			this.fromPhone = "+15005550006";
		}

		[Test]
		public void TestSendSms() {
			string toPhone = "+972545204108";
			string content = "test sms";

			Message message = this.twilioClient.SendMessage(this.fromPhone, toPhone, content);
			Assert.IsNotNull(message);
			Assert.IsNull(message.RestException);
			Assert.IsNullOrEmpty(message.ErrorMessage);
			Assert.IsNotNullOrEmpty(message.Sid);
		}

		[Test]
		public void TestGetSmsStatus() {
			string sid = "SM3525083bbed8443db5356767f7562d29";
			var message = this.twilioClient.GetMessage(sid);
			Assert.IsNotNull(message);
			Assert.IsNull(message.RestException);
			Assert.IsNullOrEmpty(message.ErrorMessage);
			Assert.AreNotEqual(message.Price, 0M);
		}

		[Test]
		public void TestRetrieveSms() {
			int page = 0, numOfPages = 0;
			do {
				var result = this.twilioClient.ListSmsMessages(this.fromPhone, null, null, page, 50);
				numOfPages = result.NumPages;
				Assert.Greater(result.SMSMessages.Count, 0);
				foreach (var msg in result.SMSMessages) {
					Console.WriteLine("from: {0} to: {1} date: {2} status: {3} body: {4}",msg.From, msg.To, msg.DateSent, msg.Status, msg.Body);
				}
				page++;
			} while (numOfPages > page);
		}

		[Test]
		public void TestLookupPhone() {
			string TwilioApiUrl = "https://lookups.twilio.com";
			string version = "v1";
			string path = String.Format("/{0}/{1}/{2}", version, "PhoneNumbers", "447402031536");

			string url;
            if (path[0] == '/')
                url = TwilioApiUrl + path;
			else
				url = TwilioApiUrl + "/" + path;

            // 2. setup basic authenication
			string authstring = Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Format("{0}:{1}", "ACcc682df6341371ee27ada6858025490b", "fab0b8bd342443ff44497273b4ba2aa1")));
            
            // 3. perform GET using WebClient
            var client = new WebClient();
            client.Headers.Add("Authorization",
                String.Format("Basic {0}", authstring));
            byte[] resp = client.DownloadData(url);
            
            var res = Encoding.ASCII.GetString(resp);

			var resObj = JsonConvert.DeserializeObject<TwilioLookupResponse>(res);

			Assert.IsNotNull(resObj);
		}
	}

	public class TwilioLookupResponse {
		public string country_code { get; set; }
		public string phone_number { get; set; }
		public string national_format { get; set; }
		public string url { get; set; }
		public Carrier carrier { get; set; }
	}

	public class Carrier {
        public string type { get; set; }
        public string error_code { get; set; }
        public string mobile_network_code { get; set; }
        public string mobile_country_code { get; set; }
		public string name { get; set; }
	}

}
