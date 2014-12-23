namespace IMailLib {
	using System;
	using System.Collections.Generic;
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
			sb.Append("Stas,Flat 1,6 Upperkirkgate,,Aberdeen,,AB10 1BA,Hello," + DateTime.Today.ToString("dd/MM/yyyy"));
			string csvStr = sb.ToString();
			byte[] csvData = System.Text.Encoding.ASCII.GetBytes(csvStr);
			bool success = api.MailmergeLetterheadPDF(PrepareMail.GetPdfData(@"c:\ezbob\test-data\imail\test4.pdf"), csvData, "<u><h3>@Variable1@ @Name@</h3><br /><h4>this is a test mail</h4></u>", false);
			if (!success) {
				Console.WriteLine(api.GetErrorMessage());
			}
			Assert.AreEqual(true, success);
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
		public void TestProcessPrintReadyPDF2() {
			TestSetPrintPreviewEmailAddress();
			
			Dictionary<string, string> varibalesDict = new Dictionary<string, string>();
			varibalesDict.Add("Name", "Stas");
			varibalesDict.Add("Address1","Flat 1");
			varibalesDict.Add("Address2", "6 Upperkirkgate");
			varibalesDict.Add("Address3","");
			varibalesDict.Add("Address4", "Aberdeen");
			varibalesDict.Add("Address5","");
			varibalesDict.Add("Postcode", "AB10 1BA");
			varibalesDict.Add("Date", "22/12/2014");

			byte[] data = PrepareMail.ReplaceParametersAndConvertToPdf(@"c:\ezbob\test-data\imail\test1.docx", varibalesDict);

			bool success = api.ProcessPrintReadyPDF(data, null, false);
			if (!success) {
					Console.WriteLine(api.GetErrorMessage());
			}
		
			Assert.AreEqual(true, success);
		}

		[Test]
		public void TestMergeMail() {
			//TestAuthenticate();
			TestSetPrintPreviewEmailAddress();
			bool success = api.UpdateAttachment(@"c:\ezbob\test-data\imail\test4.pdf", "tesstattachment3.pdf");
			if (success) {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Name,Address1,Address2,Address3,Address4,Address5,Postcode,Date,AccountNumber,AccountDate,BalanceDate,BalanceAmount,RepAmount,DueDate,Arrears");
				sb.Append("Stas,Flat 1,6 Upperkirkgate,,Aberdeen,,AB10 1BA,22/12/2014,AccountNumber,AccountDate,BalanceDate,BalanceAmount,RepAmount,DueDate,Arrears");
				string csvStr = sb.ToString();
				byte[] csvData = System.Text.Encoding.ASCII.GetBytes(csvStr);
				Console.WriteLine("data0:");
				foreach (var data in csvData) {
					Console.Write(data);
				}
				Console.WriteLine();

				//byte[] csvData2;
				//var csvPath = @"c:\ezbob\test-data\imail\test3.csv";
				//if (System.IO.File.Exists(csvPath)) {
				//	FileInfo fInfo = new FileInfo(csvPath);
				//	long numBytes = fInfo.Length;
				//	FileStream fStream = new FileStream(csvPath, FileMode.Open, FileAccess.Read);
				//	BinaryReader br = new BinaryReader(fStream);

				//	// convert the file to a byte array
				//	csvData2 = br.ReadBytes((int)numBytes);
				//	Console.WriteLine("data2:");
				//	foreach (var data in csvData2) {
				//		Console.Write(data);
				//	}
				//	Console.WriteLine();
				//	br.Close();

				//	// tidy up
				//	fStream.Close();
				//	fStream.Dispose();
				//}

				success = api.MailMerge(csvData, "tesstattachment3.pdf", false);
				if (!success) {
					Console.WriteLine(api.GetErrorMessage());
				}
				//}
			} else {
				Console.WriteLine(api.GetErrorMessage());
			}

			Assert.AreEqual(true, success);
		}

		[Test]
		public void TestPrepareMail() {
			Dictionary<string, string> varibalesDict = new Dictionary<string, string>();
			varibalesDict.Add("Name", "Stas");
			varibalesDict.Add("Address1","Flat 1");
			varibalesDict.Add("Address2", "6 Upperkirkgate");
			varibalesDict.Add("Address3","");
			varibalesDict.Add("Address4", "Aberdeen");
			varibalesDict.Add("Address5","");
			varibalesDict.Add("Postcode", "AB10 1BA");
			varibalesDict.Add("Date", "22/12/2014");

			byte[] data = PrepareMail.ReplaceParametersAndConvertToPdf(@"c:\ezbob\test-data\imail\test1.docx", varibalesDict);
			PrepareMail.SaveFile(data, @"c:\ezbob\test-data\imail\output.pdf");
		}
	}
}
