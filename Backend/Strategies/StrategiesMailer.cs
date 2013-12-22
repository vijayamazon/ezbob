using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using Aspose.Words;
using Ezbob.Database;
using Ezbob.Logger;
using MailApi;

namespace EzBob.Backend.Strategies {
	public class StrategiesMailer {
		#region public

		#region constructor

		public StrategiesMailer(AConnection oDB, ASafeLog oLog) {
			DB = oDB;
			Log = new SafeLog(oLog);

			DataTable dt = DB.ExecuteReader("GetMails", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];
			ezbobCopyTo = results["ToAddress"].ToString();
			ezbobCopyCc = results["CcAddress"].ToString();
		} // constructor

		#endregion constructor

		#region method SendToCustomerAndEzbob

		public void SendToCustomerAndEzbob(Dictionary<string, string> variables, string toAddress, string templateName, string subject) {
			SendMailViaMandrill(variables, toAddress, string.Empty, templateName, subject, true);
			SendToEzbob(variables, templateName, subject);
		} // SendToCustomerAndEzbob

		#endregion method SendToCustomerAndEzbob

		#region method SendToEzbob

		public void SendToEzbob(Dictionary<string, string> variables, string templateName, string subject) {
			SendMailViaMandrill(variables, ezbobCopyTo, ezbobCopyCc, templateName, subject, false);
		} // SendToEzbob

		#endregion method SendToEzbob

		#endregion public

		#region private

		#region method SendMailViaMandrill

		private void SendMailViaMandrill(Dictionary<string, string> variables, string toAddress, string ccAddress, string templateName, string subject, bool shouldRecord) {
			var sendStatus = mail.Send(variables, toAddress, templateName, subject, ccAddress);
			var renderedHtml = mail.GetRenderedTemplate(variables, templateName);

			if (sendStatus == null || renderedHtml == null)
				Log.Error("Failed sending mail. template:{0} to:{1} cc:{2}", templateName, toAddress, ccAddress);

			if (shouldRecord) {
				string filename = string.Format("{0}({1}).docx", subject, DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture));
				DB.ExecuteNonQuery(
					"RecordMail",
					CommandSpecies.StoredProcedure,
					new QueryParameter("Filename", filename),
					new QueryParameter("Body", HtmlToDocxBinary(renderedHtml)),
					new QueryParameter("Creation", DateTime.UtcNow),
					new QueryParameter("CustomerMail", toAddress)
				);
			} // if
		} // SendMailViaMandrill

		#endregion method SendMailViaMandrill

		#region method HtmlToDocxBinary

		private byte[] HtmlToDocxBinary(string html) {
			var doc = new Document();
			var docBuilder = new DocumentBuilder(doc);
			docBuilder.InsertHtml(html);

			using (var streamForDoc = new MemoryStream()) {
				doc.Save(streamForDoc, SaveFormat.Docx);
				return streamForDoc.ToArray();
			} // using
		} // HtmlToDocxBinary

		#endregion method HtmlToDocxBinary

		#region properties

		private readonly Mail mail = new Mail();
		private readonly string ezbobCopyTo;
		private readonly string ezbobCopyCc;

		private AConnection DB { get; set; }
		private SafeLog Log { get; set; }

		#endregion properties

		#endregion private
	} // class StrategiesMailer
} // namespace
