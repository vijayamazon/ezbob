namespace EchoSignLib {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.ServiceModel;
	using System.Text;
	using System.Text.RegularExpressions;
	using ConfigManager;
	using EchoSignService;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Exceptions;
	using Ezbob.Utils.Serialization;

	using EchoSignClient = EchoSignLib.EchoSignService.EchoSignDocumentService18PortTypeClient;
	using FileInfo = EchoSignService.FileInfo;

	public class EchoSignFacade {
		#region public

		#region constructor

		public EchoSignFacade(AConnection oDB, ASafeLog oLog) {
			m_bIsReady = false;

			m_oLog = oLog ?? new SafeLog();

			if (oDB == null)
				throw new Alert(m_oLog, "Cannot create EchoSign façade: database connection not specified.");

			m_oDB = oDB;

			try {
				LoadConfiguration();
				CreateClient();
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to initialise EchoSign façade.");
				m_bIsReady = false;
			} // try

			m_oLog.Say(m_bIsReady ? Severity.Msg : Severity.Warn, "EchoSign façade is {0}ready.", m_bIsReady ? string.Empty : "NOT ");
		} // constructor

		#endregion constructor

		#region method Send

		public bool Send(int nCustomerID, IEnumerable<int> aryDirectors, int nTemplateID, bool bSendToCustomer) {
			if (!m_bIsReady) {
				m_oLog.Msg("EchoSign cannot send - not ready.");
				return false;
			} // if

			SpLoadDataForEsign sp;

			try {
				sp = new SpLoadDataForEsign(m_oDB, m_oLog) {
					CustomerID = nCustomerID,
					TemplateID = nTemplateID,
					DirectorIDs = aryDirectors.ToList(),
				};

				sp.Load();
			}
			catch (Exception e) {
				m_oLog.Warn(e, "EchoSign cannot send: failed to load all the data from database.");
				return false;
			} // try

			if (!sp.IsReady) {
				m_oLog.Warn("EchoSign cannot send: failed to load all the data from database.");
				return false;
			} // if

			List<Person> oRecipients = new List<Person>();

			oRecipients.AddRange(sp.Directors);

			if (bSendToCustomer)
				oRecipients.Add(sp.Customer);

			if (oRecipients.Count < 1) {
				m_oLog.Warn("EchoSign cannot send: no recipients specified.");
				return false;
			} // if

			switch (sp.Template.TemplateType) {
			case TemplateType.BoardResolution:
				return SendOne(sp.Template, null, oRecipients, sp, bSendToCustomer);

			case TemplateType.PersonalGuarantee:
				bool bAllGood = true;

				foreach (Person oRecipient in oRecipients)
					if (!SendOne(sp.Template, sp.Template.PersonalGuarantee(oRecipient), new List<Person> { oRecipient }, sp, bSendToCustomer))
						bAllGood = false;

				return bAllGood;

			default:
				m_oLog.Warn("EchoSign cannot send: don't know how to send template of type {0}.", sp.Template.TemplateType);
				return false;
			} // switch
		} // Send

		private bool SendOne(Template oTemplate, byte[] oFileContent, List<Person> oAddressee, SpLoadDataForEsign oData, bool bSentToCustomer) {
			var oRecipients = oAddressee.Select(oPerson => new RecipientInfo {
				email = oPerson.Email,
				role = RecipientRole.SIGNER,
			}).ToArray();

			var sAllRecipients = string.Join(", ", oRecipients.Select(r => r.email));

			var fi = new FileInfo {
				fileName = oTemplate.FileName,
				mimeType = oTemplate.MimeType,
				file = oFileContent ?? oTemplate.FileContent,
			};

			var dci = new DocumentCreationInfo {
				name = oTemplate.DocumentName,
				signatureType = SignatureType.ESIGN,
				reminderFrequency = m_nReminderFrequency,
				signatureFlow = SignatureFlow.PARALLEL,
				daysUntilSigningDeadline = m_nDeadline,
				recipients = oRecipients,
				fileInfos = new [] { fi },
			};

			m_oLog.Debug("Sending a document '{0}' to {1}...", oTemplate.DocumentName, sAllRecipients);

			DocumentKey[] aryResult;

			try {
				aryResult = m_oEchoSign.sendDocument(m_sApiKey, null, dci);
			}
			catch (Exception e) {
				m_oLog.Warn(e, "Something went exceptionally terrible while sending a document '{0}' to {1}.", oTemplate.DocumentName, sAllRecipients);
				return false;
			} // try

			if (aryResult.Length != 1) {
				m_oLog.Alert("Failed to send documents for signing.");
			}
			else {
				m_oLog.Debug("Sending result: document key is '{0}'.", aryResult[0].documentKey);

				var sp = new SpSaveEsignSent(m_oDB, m_oLog) {
					CustomerID = oData.Customer.ID,
					Directors = oAddressee.Where(x => x.PersonType == PersonType.Director).Select(x => x.ID).ToList(),
					DocumentKey = aryResult[0].documentKey,
					SentToCustomer = bSentToCustomer,
					TemplateID = oData.Template.ID,
				};

				sp.ExecuteNonQuery();
			} // if

			return true;
		} // SendOne

		#endregion method Send

		#region method GetDocuments

		public void GetDocuments(string sDocumentKey) {
			if (!m_bIsReady) {
				m_oLog.Msg("EchoSign cannot get documents - not ready.");
				return;
			} // if

			m_oLog.Msg("Loading documents for the key '{0}' started...", sDocumentKey);

			GetDocumentsResult oResult = m_oEchoSign.getDocuments(m_sApiKey, sDocumentKey, new GetDocumentsOptions());

			m_oLog.Debug("Loading documents result:");
			m_oLog.Debug("Success: {0}.", oResult.success ? "yes" : "no");
			m_oLog.Debug("Error code: {0}.", oResult.errorCode);
			m_oLog.Debug("Error message: {0}.", oResult.errorMessage);

			int nDocumentCount = oResult.documents == null ? 0 : oResult.documents.Length;
			m_oLog.Debug("Document count: {0}.", nDocumentCount);
			m_oLog.Debug("Supporting document count: {0}.", oResult.supportingDocuments == null ? 0 : oResult.supportingDocuments.Length);

			if (nDocumentCount > 0) {
				foreach (DocumentContent doc in oResult.documents) {
					m_oLog.Debug("Document '{0}' of type '{1}', size {2} bytes.", doc.name, doc.mimetype, doc.bytes.Length);


					string sFileName = string.Format(@"c:\temp\{0}.{1}.pdf",
						DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture),
						ms_oSpecialChars.Replace(sDocumentKey, "_")
					);

					File.WriteAllBytes(sFileName, doc.bytes);
				} // foreach
			} // if

			m_oLog.Msg("Loading documents for the key '{0}' complete.", sDocumentKey);
		} // GetDocuments

		#endregion method GetDocuments

		#region method GetDocumentInfo

		public void GetDocumentInfo(string sDocumentKey) {
			if (!m_bIsReady) {
				m_oLog.Msg("EchoSign cannot get document info - not ready.");
				return;
			} // if

			m_oLog.Msg("Loading document info for the key '{0}' started...", sDocumentKey);

			DocumentInfo oResult = m_oEchoSign.getDocumentInfo(m_sApiKey, sDocumentKey);

			m_oLog.Debug("Loading document info result:");

			m_oLog.Debug("Name: {0}", oResult.name);
			m_oLog.Debug("Status: {0}", oResult.status);
			m_oLog.Debug("Expiration: {0}", oResult.expiration.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture));

			m_oLog.Msg("Loading document info for the key '{0}' complete.", sDocumentKey);
		} // GetDocumentInfo

		#endregion method GetDocumentInfo

		#endregion public

		#region private

		#region method LoadConfiguration

		private void LoadConfiguration() {
			m_sApiKey = CurrentValues.Instance.EchoSignApiKey;
			m_sUrl = CurrentValues.Instance.EchoSignUrl;

			m_nReminderFrequency = null;

			ReminderFrequency rf;
			if (Enum.TryParse(CurrentValues.Instance.EchoSignReminder, true, out rf))
				m_nReminderFrequency = rf;

			try {
				m_nDeadline = CurrentValues.Instance.EchoSignDeadline;
			}
			catch (Exception) {
				m_nDeadline = -1;
			} // try

			if (m_nDeadline < 0)
				m_nDeadline = null;

			m_oLog.Debug("************************************************************************");
			m_oLog.Debug("*");
			m_oLog.Debug("* EchoSign façade configuration - begin:");
			m_oLog.Debug("*");
			m_oLog.Debug("************************************************************************");

			m_oLog.Debug("API key: {0}.", m_sApiKey);
			m_oLog.Debug("URL: {0}.", m_sUrl);
			m_oLog.Debug("Reminder frequency: {0}.", m_nReminderFrequency.HasValue ? m_nReminderFrequency.Value.ToString() : "never");
			m_oLog.Debug("Signing deadline (days): {0}.", m_nDeadline.HasValue ? m_nDeadline.ToString() : "never");

			m_oLog.Debug("************************************************************************");
			m_oLog.Debug("*");
			m_oLog.Debug("* EchoSign façade configuration - end.");
			m_oLog.Debug("*");
			m_oLog.Debug("************************************************************************");
		} // LoadConfiguration

		#endregion method LoadConfiguration

		#region method CreateClient

		private void CreateClient() {
			m_oEchoSign = new EchoSignClient(
				new BasicHttpsBinding {
					MaxBufferSize = 65536000,
					MaxReceivedMessageSize = 65536000,
					MaxBufferPoolSize = 524288,
				},
				new EndpointAddress(m_sUrl)
			);

			m_oLog.Debug("EchoSign ping test...");

			Pong pong = m_oEchoSign.testPing(m_sApiKey);

			if (pong.message == ExpectedPong)
				m_oLog.Debug("EchoSign ping succeeded.");
			else {
				m_oLog.Alert("EchoSign ping failed! Pong is: {0}", pong.message);
				m_bIsReady = false;
				return;
			} // if

			m_oLog.Debug("EchoSign echo test...");

			byte[] oTestData = Serialized.AsBase64(string.Format("Current time is {0}.", DateTime.UtcNow.ToString("MMMM d yyyy HH:mm:ss", CultureInfo.InvariantCulture)));

			byte[] oTestResult = m_oEchoSign.testEchoFile(m_sApiKey, oTestData);

			m_bIsReady = (oTestData.Length == oTestResult.Length) && oTestData.SequenceEqual(oTestResult);

			if (m_bIsReady)
				m_oLog.Debug("EchoSign echo succeeded.");
			else
				m_oLog.Alert("EchoSign echo failed!");
		} // CreateClient

		#endregion method CreateClient

		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private bool m_bIsReady;

		private string m_sApiKey;
		private string m_sUrl;
		private ReminderFrequency? m_nReminderFrequency;
		private int? m_nDeadline;

		private EchoSignClient m_oEchoSign;

		private static readonly Regex ms_oSpecialChars = new Regex(@"[^A-Za-z0-9_-]");
		private const string ExpectedPong = "It works!";

		#endregion private
	} // class EchoSignFacade
} // namespace
