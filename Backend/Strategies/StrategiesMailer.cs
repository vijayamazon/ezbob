namespace EzBob.Backend.Strategies {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using Aspose.Words;
	using Ezbob.Database;
	using Ezbob.Logger;
	using MailApi;

	public class StrategiesMailer {
		#region public

		#region constructor

		public StrategiesMailer(AConnection oDb, ASafeLog oLog) {
			Db = oDb;
			Log = new SafeLog(oLog);

			DataTable dt = Db.ExecuteReader("GetMails", CommandSpecies.StoredProcedure);
			var sr = new SafeReader(dt.Rows[0]);
			ezbobCopyTo = sr["ToAddress"];
			ezbobCopyCc = sr["CcAddress"];
		} // constructor

		#endregion constructor

		#region method SendToCustomerAndEzbob

		public void SendToCustomerAndEzbob(Dictionary<string, string> variables, string toAddress, string templateName) {
			SendMailViaMandrill(variables, toAddress, string.Empty, templateName, true);
			SendToEzbob(variables, templateName);
		} // SendToCustomerAndEzbob

		#endregion method SendToCustomerAndEzbob

		#region method SendToEzbob

		public void SendToEzbob(Dictionary<string, string> variables, string templateName) {
			SendMailViaMandrill(variables, ezbobCopyTo, ezbobCopyCc, templateName, false);
		} // SendToEzbob

		#endregion method SendToEzbob

		#endregion public

		#region private

		#region method SendMailViaMandrill

		private void SendMailViaMandrill(Dictionary<string, string> variables, string toAddress, string ccAddress, string templateName, bool shouldRecord) {
			var sendStatus = mail.Send(variables, toAddress, templateName, string.Empty, ccAddress);
			var renderedHtml = mail.GetRenderedTemplate(variables, templateName);

			if (sendStatus == null || renderedHtml == null)
				Log.Error("Failed sending mail. template:{0} to:{1} cc:{2}", templateName, toAddress, ccAddress);

			if (shouldRecord) {
				string filename = string.Format("{0}({1}).docx", templateName, DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture));
				Db.ExecuteNonQuery(
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

		private AConnection Db { get; set; }
		private SafeLog Log { get; set; }

		#endregion properties

		#endregion private
	} // class StrategiesMailer
} // namespace
