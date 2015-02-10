namespace IMailLib {

	using System;
	using System.Collections.Generic;
	using System.Text;
	using log4net;
	using NUnit.Framework;

	[TestFixture]
	public class IMailTestFixture {

		[Test]
		public void Concatinate2Pdfs() {
			var fileData = PrepareMail.GetPdfData(@"c:\ezbob\test-data\imail\output.pdf");
			var concatData = PrepareMail.ConcatinatePdfFiles(new List<byte[]> {fileData,fileData});
			PrepareMail.SaveFile(concatData, @"c:\ezbob\test-data\imail\concatoutput.pdf");
		}

		[SetUp]
		public void SetUp() {
			api = new IMailApi();
		}

		[Test]
		public void TestAuthenticate() {
			bool isAuthenticated = api.Authenticate("Emma123456", "Ezbob123");
			Assert.AreEqual(true, isAuthenticated);
		}

		[Test]
		public void TestCollectionMails() {
			CollectionMail cm = new CollectionMail("ezbobapiuser", "Ezbob2014#", true, "stasd@ezbob.com");
			var model = new CollectionMailModel {
				CustomerAddress = new Address {
					Line1 = "cl1",
					Line2 = "cl2",
					Line3 = "cl3",
					Postcode = "AB10 1BA"
				},
				CompanyAddress = new Address {
					Line1 = "bl1",
					Line2 = "bl2",
					Line3 = "bl3",
					Postcode = "AB10 1BA"
				},
				GuarantorAddress = new Address {
					Line1 = "gl1",
					Line2 = "gl2",
					Line3 = "gl3",
					Postcode = "AB10 1BA"
				},
				CompanyName = "compname",
				CustomerId = 1,
				CustomerName = "custname",
				Date = DateTime.Today,
				GuarantorName = "guarantorname",
				LoanAmount = 20000,
				LoanDate = DateTime.Today.AddDays(-14),
				LoanRef = "loanref",
				MissedInterest = 1500,
				MissedPayment = new MissedPaymentModel {
					AmountDue = 1000,
					DateDue = DateTime.Today.AddDays(-14),
					Fees = 40,
					RepaidAmount = 0,
					RepaidDate = null
				},
				PreviousMissedPayment = new MissedPaymentModel {
					AmountDue = 1000,
					DateDue = DateTime.Today.AddMonths(-1)
						.AddDays(-14),
					Fees = 40,
					RepaidAmount = 0,
					RepaidDate = null
				},
				OutstandingBalance = 21000,
				OutstandingPrincipal = 20000,
				IsLimited = true
			};
			try {
				cm.SendDefaultNoticeComm7Borrower(model);
			} catch (Exception ex) {
				Log.InfoFormat(ex.ToString());
			}
			try {
				cm.SendDefaultTemplateComm7(model);
			} catch (Exception ex) {
				Log.InfoFormat(ex.ToString());
			}
			try {
				// cm.SendDefaultTemplateConsumer14(model);
			} catch (Exception ex) {
				Log.InfoFormat(ex.ToString());
			}
			try {
				// cm.SendDefaultTemplateConsumer31(model);
			} catch (Exception ex) {
				Log.InfoFormat(ex.ToString());
			}
			try {
				//cm.SendDefaultWarningComm7Guarantor(model);
			} catch (Exception ex) {
				Log.InfoFormat(ex.ToString());
			}
		}

		[Test]
		public void TestGetReturns() {
			TestAuthenticate();
			string returns;
			var success = api.GetReturns(out returns);
			Log.InfoFormat("returns:\n{0}", returns);
			Assert.AreEqual(true, success);
		}

		[Test]
		public void TestListAttachment() {
			TestAuthenticate();
			string attachments;
			bool success = api.ListAttachments(out attachments);
			Log.InfoFormat("attachments: {0}", attachments);
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
			if (!success)
				Log.InfoFormat(api.GetErrorMessage());
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
				
				success = api.MailMerge(csvData, "tesstattachment3.pdf", false);
				if (!success)
					Log.InfoFormat(api.GetErrorMessage());
				//}
			} else
				Log.InfoFormat(api.GetErrorMessage());

			Assert.AreEqual(true, success);
		}

		[Test]
		public void TestPrepareMail() {
			Dictionary<string, string> varibalesDict = new Dictionary<string, string>();
			varibalesDict.Add("Name", "Stas");
			varibalesDict.Add("Address1", "Flat 1");
			varibalesDict.Add("Address2", "6 Upperkirkgate");
			varibalesDict.Add("Address3", "");
			varibalesDict.Add("Address4", "Aberdeen");
			varibalesDict.Add("Address5", "");
			varibalesDict.Add("Postcode", "AB10 1BA");
			varibalesDict.Add("Date", "22/12/2014");

			byte[] data = PrepareMail.ReplaceParametersAndConvertToPdf(@"c:\ezbob\test-data\imail\test1.docx", varibalesDict);
			PrepareMail.SaveFile(data, @"c:\ezbob\test-data\imail\output.pdf");
		}

		[Test]
		public void TestProcessPrintReadyPDF() {
			TestSetPrintPreviewEmailAddress();
			bool success = api.UpdateBackground(@"c:\ezbob\test-data\imail\test1.pdf", "test1.pdf");
			if (success) {
				success = api.ProcessPrintReadyPDF(@"c:\ezbob\test-data\imail\test1.pdf", null, false);
				if (!success)
					Log.InfoFormat(api.GetErrorMessage());
			} else
				Log.InfoFormat(api.GetErrorMessage());
			Assert.AreEqual(true, success);
		}

		[Test]
		public void TestProcessPrintReadyPDF2() {
			TestSetPrintPreviewEmailAddress();

			Dictionary<string, string> varibalesDict = new Dictionary<string, string>();
			varibalesDict.Add("Name", "Stas");
			varibalesDict.Add("Address1", "Flat 1");
			varibalesDict.Add("Address2", "6 Upperkirkgate");
			varibalesDict.Add("Address3", "");
			varibalesDict.Add("Address4", "Aberdeen");
			varibalesDict.Add("Address5", "");
			varibalesDict.Add("Postcode", "AB10 1BA");
			varibalesDict.Add("Date", "22/12/2014");

			byte[] data = PrepareMail.ReplaceParametersAndConvertToPdf(@"c:\ezbob\test-data\imail\test1.docx", varibalesDict);

			bool success = api.ProcessPrintReadyPDF(data, null, false);
			if (!success)
				Log.InfoFormat(api.GetErrorMessage());

			Assert.AreEqual(true, success);
		}

		[Test]
		public void TestResource() {
			byte[] data = PrepareMail.ExtractResource("IMailLib.CollectionTemplates.default-notice-to-borrowers.docx");
			PrepareMail.SaveFile(data, @"c:\ezbob\test-data\imail\output.docx");
		}

		[Test]
		public void TestSetPrintPreviewEmailAddress() {
			TestAuthenticate();
			bool success = api.SetEmailPreview("stasd@ezbob.com");
			Assert.AreEqual(true, success);
		}

		private IMailApi api;
		private readonly ILog Log = LogManager.GetLogger(typeof (IMailTestFixture));
	}
}
