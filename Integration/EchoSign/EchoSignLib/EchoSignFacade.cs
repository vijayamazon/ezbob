﻿namespace EchoSignLib {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.ServiceModel;
	using ConfigManager;
	using EchoSignService;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Exceptions;
	using Ezbob.Utils.Serialization;

	using EchoSignClient = EchoSignLib.EchoSignService.EchoSignDocumentService18PortTypeClient;
	using FileInfo = EchoSignService.FileInfo;

	#region class EchoSignFacadeExt

	internal static class EchoSignFacadeExt {
		public static bool IsTerminal(this AgreementStatus? nStatus, SortedSet<AgreementStatus> oTerminalStatuses) {
			return (oTerminalStatuses != null) && nStatus.HasValue && oTerminalStatuses.Contains(nStatus.Value);
		} // IsTerminal
	} // class EchoSignFacadeExt

	#endregion class EchoSignFacadeExt

	#region class EchoSignFacade

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

		#endregion method Send

		#region method ProcessPending

		public void ProcessPending(int? nCustomerID = null) {
			if (!m_bIsReady) {
				m_oLog.Msg("EchoSign cannot process pending - not ready.");
				return;
			} // if

			var oSp = new SpLoadPendingEsignatures(nCustomerID, m_oDB, m_oLog);
			oSp.Load();

			foreach (KeyValuePair<int, Esignature> pair in oSp.Signatures) {
				var oSignature = pair.Value;

				GetDocumentInfo(oSignature);
			} // for each signature
		} // ProcessPending

		#endregion method ProcessPending

		#endregion public

		#region private

		#region method GetDocuments

		private SignedDoc GetDocuments(Esignature oSignature) {
			m_oLog.Msg("Loading documents for the key '{0}' started...", oSignature.DocumentKey);

			GetDocumentsResult oResult;

			try {
				oResult = m_oEchoSign.getDocuments(
					m_sApiKey,
					oSignature.DocumentKey,
					new GetDocumentsOptions {
						combine = true,
						attachSupportingDocuments = true,
					}
				);
			}
			catch (Exception e) {
				m_oLog.Warn(e, "Failed to load documents for the key '{0}'.", oSignature.DocumentKey);
				return new SignedDoc { HasValue = false, };
			} // try

			if (!oResult.success) {
				m_oLog.Warn(
					"Error while retrieving documents for the key '{0}': code = {1}, message = {2}.",
					oSignature.DocumentKey, oResult.errorCode, oResult.errorMessage
				);

				return new SignedDoc { HasValue = false, };
			} // if

			int nDocumentCount = oResult.documents == null ? 0 : oResult.documents.Length;

			if (nDocumentCount != 1) {
				m_oLog.Warn("No documents received for the key '{0}' (document count = {1}).", oSignature.DocumentKey, nDocumentCount);
				return new SignedDoc { HasValue = false, };
			} // if

			DocumentContent doc = oResult.documents[0];

			m_oLog.Debug("Document '{0}' of type '{1}', size {2} bytes.", doc.name, doc.mimetype, doc.bytes.Length);

			m_oLog.Msg("Loading documents for the key '{0}' complete.", oSignature.DocumentKey);

			return new SignedDoc { HasValue = true, MimeType = doc.mimetype, Content = doc.bytes, };
		} // GetDocuments

		#endregion method GetDocuments

		#region method GetDocumentInfo

		private void GetDocumentInfo(Esignature oSignature) {
			m_oLog.Msg("Loading document info for the key '{0}' started...", oSignature.DocumentKey);

			DocumentInfo oResult;

			try {
				oResult = m_oEchoSign.getDocumentInfo(m_sApiKey, oSignature.DocumentKey);
			}
			catch (Exception e) {
				m_oLog.Warn(e, "Failed to load document info for the '{0}'.", oSignature.DocumentKey);
				return;
			} // try

			m_oLog.Debug("Loading document info result:");

			m_oLog.Debug("Name: {0}", oResult.name);
			m_oLog.Debug("Status: {0}", oResult.status);
			m_oLog.Debug("Expiration: {0}", oResult.expiration.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture));

			if (oResult.status.HasValue) {
				// TODO: oSignature.SaveSignerStatus(oResult.events);

				SignedDoc doc = GetDocuments(oSignature);

				var sp = new SpSaveSignedDocument(m_oDB, m_oLog) {
					EsignatureID = oSignature.ID,
					StatusID = (int)oResult.status.Value,
					DoSaveDoc = doc.HasValue,
					MimeType = doc.MimeType,
					DocumentContent = doc.Content,
				};

				try {
					sp.ExecuteNonQuery();
				}
				catch (Exception e) {
					m_oLog.Alert(e, "Failed to save signed document for the key '{0}'.", oSignature.DocumentKey);
				} // try
			} // if

			m_oLog.Msg("Loading document info for the key '{0}' complete.", oSignature.DocumentKey);
		} // GetDocumentInfo

		#endregion method GetDocumentInfo

		#region method SendOne

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

		#endregion method SendOne

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

			m_oTerminalStatuses = new SortedSet<AgreementStatus>();

			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					if (!sr["IsTerminal"])
						return ActionResult.Continue;

					AgreementStatus nStatus;

					if (Enum.TryParse(sr["StatusName"], out nStatus))
						m_oTerminalStatuses.Add(nStatus);

					return ActionResult.Continue;
				},
				"LoadEsignAgreementStatuses",
				CommandSpecies.StoredProcedure
			);

			m_oLog.Debug("************************************************************************");
			m_oLog.Debug("*");
			m_oLog.Debug("* EchoSign façade configuration - begin:");
			m_oLog.Debug("*");
			m_oLog.Debug("************************************************************************");

			m_oLog.Debug("API key: {0}.", m_sApiKey);
			m_oLog.Debug("URL: {0}.", m_sUrl);
			m_oLog.Debug("Reminder frequency: {0}.", m_nReminderFrequency.HasValue ? m_nReminderFrequency.Value.ToString() : "never");
			m_oLog.Debug("Signing deadline (days): {0}.", m_nDeadline.HasValue ? m_nDeadline.ToString() : "never");
			m_oLog.Debug("Terminal statuses: {0}.", string.Join(", ", m_oTerminalStatuses));

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

		private class SignedDoc {
			public bool HasValue { get; set; }
			public string MimeType { get; set; }
			public byte[] Content { get; set; }
		} // class SignedDoc

		private SortedSet<AgreementStatus> m_oTerminalStatuses;
 
		private readonly AConnection m_oDB;
		private readonly ASafeLog m_oLog;

		private bool m_bIsReady;

		private string m_sApiKey;
		private string m_sUrl;
		private ReminderFrequency? m_nReminderFrequency;
		private int? m_nDeadline;

		private EchoSignClient m_oEchoSign;

		private const string ExpectedPong = "It works!";

		#endregion private
	} // class EchoSignFacade

	#endregion class EchoSignFacade
} // namespace
