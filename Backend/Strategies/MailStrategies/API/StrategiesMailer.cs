namespace EzBob.Backend.Strategies.MailStrategies.API {
	using System;
	using System.Collections.Generic;
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
			DB = oDb;
			Log = new SafeLog(oLog);

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

		#endregion constructor

		#region method Send

		public void Send(string templateName, Dictionary<string, string> variables, params Addressee[] aryRecipients) {
			var oMeta = new MailMetaData(templateName) { 
				new Addressee(m_sEzbobCopyTo, m_sEzbobCopyCc, false),
			};

			foreach (KeyValuePair<string, string> oVar in variables)
				oMeta.Add(oVar.Key, oVar.Value);

			foreach (var sAddr in aryRecipients)
				oMeta.Add(sAddr);

			SendMailViaMandrill(oMeta);
		} // Send

		#endregion method Send

		#region method SendMailViaMandrill

		public void SendMailViaMandrill(MailMetaData oMeta) {
			foreach (Addressee addr in oMeta) {
				var sendStatus = m_oMail.Send(oMeta, addr.Recipient, oMeta.TemplateName, string.Empty, addr.CarbonCopy);
				var renderedHtml = m_oMail.GetRenderedTemplate(oMeta, oMeta.TemplateName);

				if (sendStatus == null || renderedHtml == null)
					Log.Error("Failed sending mail. template:{0} to:{1} cc:{2}", oMeta.TemplateName, addr.Recipient, addr.CarbonCopy);

				if (addr.ShouldRegister) {
					string filename = string.Format("{0}({1}).docx", oMeta.TemplateName, DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture));

					DB.ExecuteNonQuery(
						"RecordMail",
						CommandSpecies.StoredProcedure,
						new QueryParameter("Filename", filename),
						new QueryParameter("Body", HtmlToDocxBinary(renderedHtml)),
						new QueryParameter("Creation", DateTime.UtcNow),
						new QueryParameter("CustomerMail", addr.Recipient)
					);
				} // if should register
			} // foreach
		} // SendMailViaMandrill

		#endregion method SendMailViaMandrill

		#endregion public

		#region private

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

		private readonly Mail m_oMail;
		private string m_sEzbobCopyTo;
		private string m_sEzbobCopyCc;

		private AConnection DB { get; set; }
		private SafeLog Log { get; set; }

		#endregion properties

		#endregion private
	} // class StrategiesMailer
} // namespace
