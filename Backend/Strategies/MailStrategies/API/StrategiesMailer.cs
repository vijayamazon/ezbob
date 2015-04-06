﻿namespace Ezbob.Backend.Strategies.MailStrategies.API {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using Aspose.Words;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailApi;
	using SalesForceLib.Models;

	public class StrategiesMailer {

		public StrategiesMailer() {
			DB = Library.Instance.DB;
			Log = Library.Instance.Log.Safe();

			m_oMail = new Mail();

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					m_sEzbobCopyTo = sr["ToAddress"];
					m_sEzbobCopyCc = sr["CcAddress"];
					return ActionResult.SkipAll;
				},
				"GetMails",
				CommandSpecies.StoredProcedure
			);
		} // constructor

		public void Send(string templateName, Dictionary<string, string> variables, params Addressee[] aryRecipients) {
			var oMeta = new MailMetaData(templateName) {
				new Addressee(m_sEzbobCopyTo, m_sEzbobCopyCc, false)
			};

			Send(oMeta, variables, aryRecipients);
		} // Send

		public void Send(MailMetaData meta, Dictionary<string, string> variables, params Addressee[] aryRecipients) {
			foreach (KeyValuePair<string, string> oVar in variables)
				meta.Add(oVar.Key, oVar.Value);

			foreach (var sAddr in aryRecipients)
				meta.Add(sAddr);

			SendMailViaMandrill(meta);
		} // Send

		public void SendMailViaMandrill(MailMetaData oMeta) {
			foreach (Addressee addr in oMeta) {
				var sendStatus = m_oMail.Send(oMeta, addr.Recipient, oMeta.TemplateName, string.Empty, addr.CarbonCopy);
				var renderedHtml = m_oMail.GetRenderedTemplate(oMeta, oMeta.TemplateName);
				var now = DateTime.UtcNow;
				if (sendStatus == null || renderedHtml == null)
					Log.Error("Failed sending mail. template:{0} to:{1} cc:{2}", oMeta.TemplateName, addr.Recipient, addr.CarbonCopy);

				if (addr.ShouldRegister) {
					string filename = string.Format("{0}({1}).html", oMeta.TemplateName, DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture));

					DB.ExecuteNonQuery(
						"RecordMail",
						CommandSpecies.StoredProcedure,
						new QueryParameter("Filename", filename),
						new QueryParameter("Body", HtmlToBinary(renderedHtml)),
						new QueryParameter("Creation", DateTime.UtcNow),
						new QueryParameter("CustomerMail", addr.Recipient),
						new QueryParameter("UserID", addr.UserID),
						new QueryParameter("TemplateName", oMeta.TemplateName)
					);

				    if (!addr.IsBroker) {
				        AddSalesForceActivity(now, oMeta, addr);
				    }
				} // if should register
			} // foreach
		}

	    private void AddSalesForceActivity(DateTime now, MailMetaData oMeta, Addressee addr) {
	        try {
	            var salesForceAddEvent = new AddActivity(null, new ActivityModel {
	                StartDate = now,
	                EndDate = now,
	                Description = oMeta.TemplateName,
	                Email = addr.Recipient,
	                Originator = "System",
	                Type = ActivityType.Email.ToString(),
	                IsOpportunity = false,
	            });
	            salesForceAddEvent.Execute();
            } catch (Exception ex) {
                Log.Error(ex, "Failed to SF add activity to {0}", addr.Recipient);
            }
	    }

// SendMailViaMandrill

		private byte[] HtmlToDocxBinary(string html) {
			if (html == null)
				return new byte[0];

			var doc = new Document();
			var docBuilder = new DocumentBuilder(doc);
			docBuilder.InsertHtml(html);

			using (var streamForDoc = new MemoryStream()) {
				doc.Save(streamForDoc, SaveFormat.Docx);
				return streamForDoc.ToArray();
			} // using
		} // HtmlToDocxBinary

		private byte[] HtmlToBinary(string html) {
			if (html == null)
				return new byte[0];

			return System.Text.Encoding.Default.GetBytes(html);
		} // HtmlToDocxBinary

		private readonly Mail m_oMail;
		private string m_sEzbobCopyTo;
		private string m_sEzbobCopyCc;

		private AConnection DB { get; set; }
		private ASafeLog Log { get; set; }

	} // class StrategiesMailer
} // namespace
