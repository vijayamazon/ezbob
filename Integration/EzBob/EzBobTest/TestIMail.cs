﻿namespace EzBobTest {
	using IMailLib;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Ezbob.Database.Pool;
	using Ezbob.Logger;
	using log4net;
	using NUnit.Framework;

	[TestFixture]
	public class TestIMail {
		[SetUp]
		public void SetUp() {
			this.api = new IMailApi();
			var oLog4NetCfg = new Log4Net().Init();

			this.Env = oLog4NetCfg.Environment;
			this.ALog = new SafeILog(Log);
			this.DB = new SqlConnection(oLog4NetCfg.Environment, this.ALog);

			ConfigManager.CurrentValues.Init(this.DB, this.ALog);
			DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
			AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);
		}

		[Test]
		public void TestLoadTemplatesFromDB() {
			List<CollectionSnailMailTemplate> templates = this.DB.Fill<CollectionSnailMailTemplate>("LoadCollectionSnailMailTemplates", CommandSpecies.StoredProcedure);
			Assert.IsTrue(templates.Any());
		}

		[Test]
		public void Concatinate2Pdfs() {
			var fileData = PrepareMail.GetPdfData(@"c:\ezbob\test-data\imail\output.pdf");
			var concatData = PrepareMail.ConcatinatePdfFiles(new List<byte[]> { fileData, fileData });
			PrepareMail.SaveFile(concatData, @"c:\ezbob\test-data\imail\concatoutput.pdf");
		}

		[Test]
		public void TestAuthenticate() {
			bool isAuthenticated = this.api.Authenticate("Emma123456", "Ezbob123");
			Assert.AreEqual(true, isAuthenticated);
		}

		[Test]
		public void TestCollectionMails() {
			TestCollectionMails(1);
			TestCollectionMails(2);
		}
		
		private void TestCollectionMails(int originId) {
			CollectionMail cm = new CollectionMail("ezbobapiuser", "Ezbob2014#", true, "stasd@ezbob.com");
			List<CollectionSnailMailTemplate> templates = this.DB.Fill<CollectionSnailMailTemplate>("LoadCollectionSnailMailTemplates", CommandSpecies.StoredProcedure);
			cm.SetTemplates(templates.Select(x => new SnailMailTemplate {
				Type = x.Type,
				OriginID = x.OriginID,
				Template = x.Template,
				IsActive = x.IsActive,
				TemplateName = x.TemplateName,
				FileName = x.FileName,
				IsLimited = x.IsLimited
			}));
			var model = GetMailModel(true, originId);
			var consumerModel = GetMailModel(false, originId);

			try {
				cm.SendDefaultNoticeComm14Borrower(model);
			} catch (Exception ex) {
				Log.InfoFormat(ex.ToString());
			}
			try {
				cm.SendDefaultTemplateComm7(model);
			} catch (Exception ex) {
				Log.InfoFormat(ex.ToString());
			}
			
			try {
				cm.SendDefaultTemplateConsumer14(consumerModel);
			} catch (Exception ex) {
				Log.InfoFormat(ex.ToString());
			}
			try {
				cm.SendDefaultTemplateConsumer31(consumerModel);
			} catch (Exception ex) {
				Log.InfoFormat(ex.ToString());
			}
			try {
				cm.SendDefaultWarningComm7Guarantor(model);
			} catch (Exception ex) {
				Log.InfoFormat(ex.ToString());
			}
		}

		[Test]
		public void TestGetReturns() {
			TestAuthenticate();
			string returns;
			var success = this.api.GetReturns(out returns);
			Log.InfoFormat("returns:\n{0}", returns);
			Assert.AreEqual(true, success);
		}

		[Test]
		public void TestListAttachment() {
			TestAuthenticate();
			string attachments;
			bool success = this.api.ListAttachments(out attachments);
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
			bool success = this.api.MailmergeLetterheadPDF(PrepareMail.GetPdfData(@"c:\ezbob\test-data\imail\test4.pdf"), csvData, "<u><h3>@Variable1@ @Name@</h3><br /><h4>this is a test mail</h4></u>", false);
			if (!success)
				Log.InfoFormat(this.api.GetErrorMessage());
			Assert.AreEqual(true, success);
		}

		[Test]
		public void TestMergeMail() {
			//TestAuthenticate();
			TestSetPrintPreviewEmailAddress();
			bool success = this.api.UpdateAttachment(@"c:\ezbob\test-data\imail\test4.pdf", "tesstattachment3.pdf");
			if (success) {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Name,Address1,Address2,Address3,Address4,Address5,Postcode,Date,AccountNumber,AccountDate,BalanceDate,BalanceAmount,RepAmount,DueDate,Arrears");
				sb.Append("Stas,Flat 1,6 Upperkirkgate,,Aberdeen,,AB10 1BA,22/12/2014,AccountNumber,AccountDate,BalanceDate,BalanceAmount,RepAmount,DueDate,Arrears");
				string csvStr = sb.ToString();
				byte[] csvData = System.Text.Encoding.ASCII.GetBytes(csvStr);

				success = this.api.MailMerge(csvData, "tesstattachment3.pdf", false);
				if (!success)
					Log.InfoFormat(this.api.GetErrorMessage());
				//}
			} else
				Log.InfoFormat(this.api.GetErrorMessage());

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
			bool success = this.api.UpdateBackground(@"c:\ezbob\test-data\imail\test1.pdf", "test1.pdf");
			if (success) {
				success = this.api.ProcessPrintReadyPDF(@"c:\ezbob\test-data\imail\test1.pdf", null, false);
				if (!success)
					Log.InfoFormat(this.api.GetErrorMessage());
			} else
				Log.InfoFormat(this.api.GetErrorMessage());
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

			bool success = this.api.ProcessPrintReadyPDF(data, null, false);
			if (!success)
				Log.InfoFormat(this.api.GetErrorMessage());

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
			bool success = this.api.SetEmailPreview("stasd@ezbob.com");
			Assert.AreEqual(true, success);
		}

		private CollectionMailModel GetMailModel(bool isLimited, int originId) {
			var model = new CollectionMailModel {
				IsLimited = isLimited,
				OriginId = originId,
				CustomerAddress = new Address {
					Line1 = "6 Upperkirkgate",
					Line2 = "cl2",
					Line3 = "cl3",
					Line4 = "Aberdeen",
					Postcode = "AB10 1BA"
				},
				CompanyAddress = new Address {
					Line1 = "6 Upperkirkgate",
					Line2 = "bl2",
					Line3 = "bl3",
					Line4 = "Aberdeen",
					Postcode = "AB10 1BA"
				},
				GuarantorAddress = new Address {
					Line1 = "6 Upperkirkgate",
					Line2 = "gl2",
					Line3 = "gl3",
					Line4 = "Aberdeen",
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
			};
			return model;
		}

		private IMailApi api;
		protected static readonly ILog Log = LogManager.GetLogger(typeof (TestIMail));
		protected AConnection DB;
		protected ASafeLog ALog;
		protected Ezbob.Context.Environment Env;
	}
}
