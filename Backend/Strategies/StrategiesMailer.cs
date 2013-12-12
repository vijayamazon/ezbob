namespace EzBob.Backend.Strategies
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using Aspose.Words;
	using DbConnection;
	using MailApi;
	using log4net;
	using System.Collections.Generic;

	public class StrategiesMailer
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(StrategiesMailer));
		private readonly Mail mail = new Mail();

		private void SendMailViaMandrill(Dictionary<string, string> variables, string toAddress, string ccAddress, string templateName, string subject, bool shouldRecord)
		{
			var sendStatus = mail.Send(variables, toAddress, templateName, subject, ccAddress);
			var renderedHtml = mail.GetRenderedTemplate(variables, templateName);

			if (sendStatus == null || renderedHtml == null)
			{
				log.ErrorFormat("Failed sending mail. template:{0} to:{1} cc:{2}", templateName, toAddress, ccAddress);
			}

			if (shouldRecord)
			{
				string filename = string.Format("{0}({1}).docx", subject, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture));
				DbConnection.ExecuteSpNonQuery("RecordMail",
				    DbConnection.CreateParam("Filename", filename),
				    DbConnection.CreateParam("Body", HtmlToDocxBinary(renderedHtml)),
				    DbConnection.CreateParam("Creation", DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)),
				    DbConnection.CreateParam("CustomerMail", toAddress));
			}
		}

		private byte[] HtmlToDocxBinary(string html)
		{
			var doc = new Document();
			var docBuilder = new DocumentBuilder(doc);
			docBuilder.InsertHtml(html);

			using (var streamForDoc = new MemoryStream())
			{
				doc.Save(streamForDoc, SaveFormat.Docx);
				return streamForDoc.ToArray();
			}
		}

		public void SendToCustomerAndEzbob(Dictionary<string, string> variables, string toAddress, string templateName, string subject)
		{
			SendMailViaMandrill(variables, toAddress, string.Empty, templateName, subject, true);
			SendToEzbob(variables, templateName, subject);
		}

		public void SendToEzbob(Dictionary<string, string> variables, string templateName, string subject)
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetMails");
			DataRow results = dt.Rows[0];
			string ezbobCopyTo = results["EzbobMailTo"].ToString();
			string ezbobCopyCc = results["EzbobMailCc"].ToString();
			SendMailViaMandrill(variables, ezbobCopyTo, ezbobCopyCc, templateName, subject, false);
		}
	}
}
