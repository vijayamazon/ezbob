namespace IMailLib {
	using System;
	using System.Text;
	using NUnit.Framework;

	[TestFixture]
	public class IMailTestFixture {
		private IMailApi api;
		[SetUp]
		public void SetUp(){
			api = new IMailApi();
		}

		[Test]
		public void TestAuthenticate() {
			bool isAuthenticated = api.Authenticate("Emma123456", "Ezbob123");
			Assert.AreEqual(true, isAuthenticated);
		}

		[Test]
		public void TestListAttachment() {
			TestAuthenticate();
			string attachments;
			bool success = api.ListAttachments(out attachments);
			Console.WriteLine("attachments: {0}", attachments);
			Assert.AreEqual(true, success);
		}

		[Test]
		public void TestGetReturns() {
			TestAuthenticate();
			string returns;
			var success = api.GetReturns(out returns);
			Console.WriteLine("returns:\n{0}", returns);
			Assert.AreEqual(true, success);
		}

		[Test]
		public void TestSetPrintPreviewEmailAddress() {
			TestAuthenticate();
			bool success = api.SetEmailPreview("stasd@ezbob.com");
			Assert.AreEqual(true, success);
		}

		[Test]
		public void TestMailmergeLetterheadPDF() {
			TestSetPrintPreviewEmailAddress();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Name,Address1,Address2,Address3,Address4,Address5,Postcode,Variable1,Date");
			sb.AppendLine("Stas,Flat 1,6 Upperkirkgate,,Aberdeen,,AB10 1BA,Hello," + DateTime.Today.ToString("dd/MM/yyyy"));
			string csvStr = sb.ToString();
			byte[] data = System.Text.Encoding.ASCII.GetBytes(csvStr);
			string dataStr = Convert.ToBase64String(data);
			byte[] csvData = System.Text.Encoding.ASCII.GetBytes(dataStr);
			bool success = api.MailmergeLetterheadPDF(null, csvData, "<u><h3>@Variable1@ @Name@</h3><br /><h4>this is a test mail</h4></u>", false);
		}

		[Test]
		public void TestProcessPrintReadyPDF() {
			TestSetPrintPreviewEmailAddress();
			bool success = api.UpdateBackground(@"c:\ezbob\test-data\imail\test1.pdf", "test1.pdf");
			if (success) {
				success = api.ProcessPrintReadyPDF(@"c:\ezbob\test-data\imail\test1.pdf", null, false);
				if (!success) {
					Console.WriteLine(api.GetErrorMessage());
				}
			} else {
				Console.WriteLine(api.GetErrorMessage());
			}
			Assert.AreEqual(true, success);
		}

		[Test]
		public void TestMergeMail() {
			TestSetPrintPreviewEmailAddress();
			bool success = api.UpdateAttachment(@"c:\ezbob\test-data\imail\test2.pdf", "tesstattachment.pdf");
			if (success) {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Name,Address1,Address2,Address3,Address4,Address5,Postcode,Variable1,Date");
				sb.AppendLine("Stas,Flat 1,6 Upperkirkgate,,Aberdeen,,AB10 1BA,Hello," + DateTime.Today.ToString("dd/MM/yyyy"));
				string csvStr = sb.ToString();
				byte[] data = System.Text.Encoding.ASCII.GetBytes(csvStr);
				string dataStr = Convert.ToBase64String(data);
				byte[] csvData = System.Text.Encoding.ASCII.GetBytes(dataStr);
				success = api.MailMerge(csvData, "tesstattachment.pdf", false);
				if (success) {
					success = api.ProcessPrintReadyPDF(@"c:\ezbob\test-data\imail\test2.pdf", null, false);
					if (!success) {
						Console.WriteLine(api.GetErrorMessage());
					}
				} else {
					Console.WriteLine(api.GetErrorMessage());
				}

			} else {
				Console.WriteLine(api.GetErrorMessage());
			}

			Assert.AreEqual(true, success);
		}
	}
}
