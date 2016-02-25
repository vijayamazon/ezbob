namespace EchoSignLib {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.ServiceModel;
	using ConfigManager;
	using EchoSignLib.Internal;
	using EchoSignLib.Rest;
	using EchoSignLib.Rest.Api;
	using EchoSignLib.Rest.Models;
	using EchoSignService;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Exceptions;
	using Ezbob.Utils.Lingvo;
	using Ezbob.Utils.Serialization;

	using EchoSignClient = EchoSignLib.EchoSignService.EchoSignDocumentService19PortTypeClient;
	using FileInfo = EchoSignService.FileInfo;

	internal static class EchoSignFacadeExt {
		public static bool IsTerminal(this AgreementStatus? nStatus, SortedSet<AgreementStatus> oTerminalStatuses) {
			return (oTerminalStatuses != null) && nStatus.HasValue && oTerminalStatuses.Contains(nStatus.Value);
		} // IsTerminal
	} // class EchoSignFacadeExt

	public class EchoSignFacade {

        private HashSet<int> terminalStatuses;

        private readonly AConnection db;
        private readonly ASafeLog log;

        private bool isReady;

        private string apiKey;
        private string url;
        private ReminderFrequency? reminderFrequency;
        private int? deadline;

        private EchoSignClient echoSign;

        private const string ExpectedPong = "It works!";

	    private bool isUseRestApi = true;

	    private readonly EchoSignRestClient restClient;

        private class SignedDoc
        {
            public bool HasValue { get; set; }
            public string MimeType { get; set; }
            public byte[] Content { get; set; }
        } // class SignedDoc

		public EchoSignFacade(AConnection oDB, ASafeLog oLog) {
			this.isReady = false;

			this.log = oLog.Safe();

			if (oDB == null)
				throw new Alert(this.log, "Cannot create EchoSign façade: database connection not specified.");

			this.db = oDB;

			try {
				LoadConfiguration();
				CreateClient();
			} catch (Exception e) {
				this.log.Alert(e, "Failed to initialize EchoSign façade.");
				this.isReady = false;
			} // try

			this.log.Say(
				this.isReady ? Severity.Msg : Severity.Warn,
				"EchoSign façade is {0}ready.",
				this.isReady ? string.Empty : "NOT "
			);

//            this.restClient = new EchoSignRestClient("3AAABLblqZhABY-QxNfpgWJizd4ybsQa4hakBWBma-TYNXRyJYtAvqcU6TjK-zqtPRF4TqM-kLw0*", "96A2AM24676Z7M", "98867629f92bfc28c8fd8df662d74626", "https://redirect.ezbob.com");
		    this.restClient = new EchoSignRestClient(CurrentValues.Instance.EchoSignRefreshToken, CurrentValues.Instance.EchoSignClientId, CurrentValues.Instance.EchoSignClientSecret, CurrentValues.Instance.EchoSignRedirectUri);
		} // constructor

		public EchoSignSendResult Send(IEnumerable<EchoSignEnvelope> oCorrespondence) {
			var result = new EchoSignSendResult();

			int nTotalCount = 0;
			int nSuccessCount = 0;

			foreach (var oLetter in oCorrespondence) {
				nTotalCount++;

				if (EchoSignSendResultCode.Success == Send(oLetter, result))
					nSuccessCount++;
			} // for each

			if (nSuccessCount == 0) {
				result.Code = EchoSignSendResultCode.Fail;
				return result;
			} // if

			result.Code = nTotalCount == nSuccessCount ? EchoSignSendResultCode.Success : EchoSignSendResultCode.Partial;
			return result;
		} // Send

		public List<EsignatureStatus> ProcessPending(int? nCustomerID = null) {
			if (!this.isReady) {
				this.log.Msg("EchoSign cannot process pending - not ready.");
				return new List<EsignatureStatus>();
			} // if

			var oSp = new SpLoadPendingEsignatures(nCustomerID, this.db, this.log);
			oSp.Load();

			this.log.Debug(
				"{0} pending signature{1} discovered.",
				oSp.Signatures.Count,
				oSp.Signatures.Count == 1 ? "" : "s"
			);

			var oCompleted = new List<EsignatureStatus>();

			foreach (KeyValuePair<int, Esignature> pair in oSp.Signatures) {
				Esignature oSignature = pair.Value;

				int nStatus = GetAgreementStatus(oSignature);

				if (this.terminalStatuses.Contains(nStatus)) {
					this.log.Debug(
						"Terminal status {0} for e-signature {1} (customer {2}).",
						nStatus, oSignature.ID, oSignature.CustomerID
					);

					oCompleted.Add(new EsignatureStatus {
						CustomerID = oSignature.CustomerID,
						EsignatureID = oSignature.ID,
						Status = (AgreementStatus)nStatus
					});
				} // if
			} // for each signature

			this.log.Debug(
				"{0} processed, out of them {1} ha{2} completed.",
				Grammar.Number(oSp.Signatures.Count, "pending signature"),
				oCompleted.Count,
				oCompleted.Count == 1 ? "ve" : "s"
			);

			return oCompleted;
		} // ProcessPending

		private EchoSignSendResultCode Send(EchoSignEnvelope oLetter, EchoSignSendResult result) {
			if (!this.isReady) {
				const string msg = "EchoSign cannot send - not ready.";
				this.log.Warn("{0}", msg);
				result.AddErrorMessage(msg);
				return EchoSignSendResultCode.Fail;
			} // if

			if (oLetter == null) {
				const string msg = "NULL EchoSign request discovered.";
				this.log.Warn("{0}", msg);
				result.AddErrorMessage(msg);
				return EchoSignSendResultCode.Fail;
			} // if

			if (!oLetter.IsValid) {
				string msg = string.Format("Some data are missing in EchoSign request: {0}.", oLetter);
				this.log.Warn("{0}", msg);
				result.AddErrorMessage(msg);
				return EchoSignSendResultCode.Fail;
			} // if

			return Send(
				oLetter.CustomerID,
				oLetter.CashRequestID,
				oLetter.Directors ?? new int[0],
				oLetter.ExperianDirectors ?? new int[0],
				oLetter.TemplateID,
				oLetter.SendToCustomer,
				result
			);
		} // Send

		private EchoSignSendResultCode Send(
			int nCustomerID,
			long cashRequestID,
			IEnumerable<int> aryDirectors,
			IEnumerable<int> aryExperianDirectors,
			int nTemplateID,
			bool bSendToCustomer,
			EchoSignSendResult result
		) {
			SpLoadDataForEsign sp;

			try {
				sp = new SpLoadDataForEsign(this.db, this.log) {
					CustomerID = nCustomerID,
					TemplateID = nTemplateID,
					DirectorIDs = aryDirectors.ToList(),
					ExperianDirectorIDs = aryExperianDirectors.ToList(),
				};

				sp.Load();
			} catch (Exception e) {
				const string msg = "EchoSign cannot send: failed to load all the data from database.";
				this.log.Warn(e, msg);
				result.AddErrorMessage("{0} {1}", msg, e.Message);
				return EchoSignSendResultCode.Fail;
			} // try

			if (!sp.IsReady) {
				string msg = string.Format(
					"EchoSign cannot send: failed to load all the data from database.\n{0}",
					string.Join("\n", sp.ErrorList)
				);

				this.log.Warn(msg);
				result.AddErrorMessage(msg);
				return EchoSignSendResultCode.Fail;
			} // if

			List<Person> oRecipients = new List<Person>();

			oRecipients.AddRange(sp.Directors);
			oRecipients.AddRange(sp.ExperianDirectors);

			if (bSendToCustomer)
				oRecipients.Add(sp.Customer);

			if (oRecipients.Count < 1) {
				const string msg = "EchoSign cannot send: no recipients specified.";
				this.log.Warn(msg);
				result.AddErrorMessage(msg);
				return EchoSignSendResultCode.Fail;
			} // if

			switch (sp.Template.TemplateType) {
			case TemplateType.BoardResolution:
				return SendOne(
					sp.Template,
					null,
					oRecipients,
					sp.Customer.ID,
					cashRequestID,
					sp.Template.ID,
					bSendToCustomer,
					result
				);

			case TemplateType.PersonalGuarantee:
				int nApprovedSum = this.db.ExecuteScalar<int>(
					"LoadCustomerLatestApprovedSum",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", sp.Customer.ID)
				);

				if (nApprovedSum <= 0) {
					const string msg = "EchoSign cannot send: approved sum is not positive.";
					this.log.Warn(msg);
					result.AddErrorMessage(msg);
					return EchoSignSendResultCode.Fail;
				} // if

				int nTotalCount = 0;
				int nSuccessCount = 0;

				foreach (Person oRecipient in oRecipients) {
					bool bIsCustomer = ReferenceEquals(oRecipient, sp.Customer);

					if (bIsCustomer && !bSendToCustomer)
						continue;

					nTotalCount++;

					EchoSignSendResultCode bSendOneResult = SendOne(
						sp.Template,
						sp.Template.PersonalGuarantee(oRecipient, nApprovedSum),
						new List<Person> { oRecipient },
						sp.Customer.ID,
						cashRequestID,
						sp.Template.ID,
						bIsCustomer,
						result
					);

					if (EchoSignSendResultCode.Success == bSendOneResult)
						nSuccessCount++;
				} // for each

				if (nSuccessCount == 0) {
					result.AddErrorMessage("No documents were sent.");
					return EchoSignSendResultCode.Fail;
				} // if

				if (nTotalCount == nSuccessCount)
					return EchoSignSendResultCode.Success;
				else {
					result.AddErrorMessage("Some documents were not sent.");
					return EchoSignSendResultCode.Partial;
				} // if

			default: {
					string msg = string.Format(
						"EchoSign cannot send: don't know how to send template of type {0}.",
						sp.Template.TemplateType
					);
					this.log.Warn("{0}", msg);
					result.AddErrorMessage(msg);
					return EchoSignSendResultCode.Fail;
				}
			} // switch
		} // Send

		private SignedDoc GetDocuments(Esignature oSignature) {
			this.log.Msg("Loading documents for the key '{0}' started...", oSignature.DocumentKey);

			GetDocumentsResult oResult;

			try {
				oResult = this.echoSign.getDocuments(
					this.apiKey,
					oSignature.DocumentKey,
					new GetDocumentsOptions {
						combine = true,
						attachSupportingDocuments = true,
					}
				);
			} catch (Exception e) {
				this.log.Warn(e, "Failed to load documents for the key '{0}'.", oSignature.DocumentKey);
				return new SignedDoc { HasValue = false, };
			} // try

			if (!oResult.success) {
				this.log.Warn(
					"Error while retrieving documents for the key '{0}': code = {1}, message = {2}.",
					oSignature.DocumentKey, oResult.errorCode, oResult.errorMessage
				);

				return new SignedDoc { HasValue = false, };
			} // if

			int nDocumentCount = oResult.documents == null ? 0 : oResult.documents.Length;

			if (nDocumentCount != 1) {
				this.log.Warn(
					"No documents received for the key '{0}' (document count = {1}).",
					oSignature.DocumentKey,
					nDocumentCount
				);
				return new SignedDoc { HasValue = false, };
			} // if

			// ReSharper disable once PossibleNullReferenceException
			// Can be disabled because of documentCount != 1 check just above.
			DocumentContent doc = oResult.documents[0];

			this.log.Debug("Document '{0}' of type '{1}', size {2} bytes.", doc.name, doc.mimetype, doc.bytes.Length);

			this.log.Msg("Loading documents for the key '{0}' complete.", oSignature.DocumentKey);

			return new SignedDoc { HasValue = true, MimeType = doc.mimetype, Content = doc.bytes, };
		} // GetDocuments

	    private int GetAgreementStatus(Esignature oSignature) {
	        if (this.isUseRestApi) {
	            return HandleRestAgreementStatus(oSignature);
	        }
            
	        var status = HandleSoapAgreementStatus(oSignature);
	        if (status.HasValue) {
	            return (int)status.Value;
	        }

	        return -1;
	    }

        private int HandleRestAgreementStatus(Esignature oSignature)
        {
			this.log.Msg("Loading document info for the key '{0}' started...", oSignature.DocumentKey);

			DocumentInfo oResult;
		    int agreementStatus = -1;

		    EchoSignAgreementStatusResponse response;
			try {
			    response = this.restClient.GetAgreementStatus(oSignature.DocumentKey).Result;
			    agreementStatus = (int)response.status;

			    oResult = new DocumentInfo {
			        documentKey = response.agreementId,
                    name = response.name,
                    message = response.message,
                    expiration = response.expiration ?? DateTime.MinValue,
			    };
			} catch (Exception e) {
				this.log.Warn(e, "Failed to load document info for the '{0}'.", oSignature.DocumentKey);
				return -1;
			} // try

			this.log.Debug("Loading document info result:");
			this.log.Debug("Name: {0}", oResult.name);
			this.log.Debug("Status: {0}", response.status);
			this.log.Debug(
				"Expiration: {0}",
				oResult.expiration.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture)
			);

            oSignature.SetHistoryAndStatus(response.events, response.participantSetInfos);

            EchoSignAgreementDocumentResponse docResponse = this.restClient.GetAgreementDocument(oSignature.DocumentKey).Result;

            var sp = new SpSaveSignedDocument(this.db, this.log)
            {
                EsignatureID = oSignature.ID,
                StatusID = agreementStatus,
                DoSaveDoc = true,
                MimeType = docResponse.MimeType,
                DocumentContent = docResponse.Content,
                SignerStatuses = oSignature.SignerStatuses,
                HistoryEvents = oSignature.HistoryEvents,
            };

            try
            {
                sp.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                this.log.Alert(e, "Failed to save signed document for the key '{0}'.", oSignature.DocumentKey);
            } // try

			this.log.Msg("Loading document info for the key '{0}' complete.", oSignature.DocumentKey);

			return agreementStatus;
		} // GetAgreementStatus

        private AgreementStatus? HandleSoapAgreementStatus(Esignature oSignature)
        {
            this.log.Msg("Loading document info for the key '{0}' started...", oSignature.DocumentKey);

            DocumentInfo oResult;

            try
            {
                oResult = this.echoSign.getDocumentInfo(this.apiKey, oSignature.DocumentKey);
            }
            catch (Exception e)
            {
                this.log.Warn(e, "Failed to load document info for the '{0}'.", oSignature.DocumentKey);
                return null;
            } // try

            this.log.Debug("Loading document info result:");
            this.log.Debug("Name: {0}", oResult.name);
            this.log.Debug("Status: {0}", oResult.status);
            this.log.Debug(
                "Expiration: {0}",
                oResult.expiration.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture)
            );

            if (oResult.status.HasValue)
            {
                SignedDoc doc = GetDocuments(oSignature);

                oSignature.SetHistoryAndStatus(oResult.events, oResult.participants);

                if (doc.HasValue)
                {
                    var sp = new SpSaveSignedDocument(this.db, this.log)
                    {
                        EsignatureID = oSignature.ID,
                        StatusID = (int)oResult.status.Value,
                        DoSaveDoc = doc.HasValue,
                        MimeType = doc.MimeType,
                        DocumentContent = doc.Content,
                        SignerStatuses = oSignature.SignerStatuses,
                        HistoryEvents = oSignature.HistoryEvents,
                    };

                    try
                    {
                        sp.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        this.log.Alert(e, "Failed to save signed document for the key '{0}'.", oSignature.DocumentKey);
                    } // try
                }
                else
                {
                    this.log.Debug(
                        "Nothing to save for the key '{0}': no documents received from EchoSign.",
                        oSignature.DocumentKey
                    );
                } // if
            } // if

            this.log.Msg("Loading document info for the key '{0}' complete.", oSignature.DocumentKey);

            return oResult.status;
	    }

		private EchoSignSendResultCode SendOne(
			Template oTemplate,
			byte[] oFileContent,
			List<Person> oAddressee,
			int nCustomerID,
			long cashRequestID,
			int nTemplateID,
			bool bSentToCustomer,
			EchoSignSendResult result
		) {
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
				reminderFrequency = this.reminderFrequency,
				signatureFlow = SignatureFlow.PARALLEL,
				daysUntilSigningDeadline = this.deadline,
				recipients = oRecipients,
				fileInfos = new[] { fi },
			};

			this.log.Debug("Sending a document '{0}' to {1}...", oTemplate.DocumentName, sAllRecipients);

			DocumentKey[] aryResult;

			try {
			    if (this.isUseRestApi) {
			        var response = this.restClient.SendAgreement(dci).Result;
			        if (string.IsNullOrEmpty(response.agreementId)) {
			            throw new Exception("error sending when sent agreement");
			        }
			        DocumentKey documentKey = new DocumentKey {
			            documentKey = response.agreementId
			        };

			        aryResult = new[] {
			            documentKey
			        };

			    } else {
			        aryResult = this.echoSign.sendDocument(this.apiKey, null, dci);
			    }
			} catch (Exception e) {
				string msg = string.Format(
					"Something went exceptionally terrible while sending a document '{0}' to {1}.",
					oTemplate.DocumentName,
					sAllRecipients
				);
				this.log.Warn(e, msg);
				result.AddErrorMessage(msg);
				return EchoSignSendResultCode.Fail;
			} // try

			if (aryResult.Length != 1) {
				const string msg = "Failed to send documents for signing.";
				this.log.Alert(msg);
				result.AddErrorMessage(msg);
			} else {
				this.log.Debug("Sending result: document key is '{0}'.", aryResult[0].documentKey);

				var sp = new SpSaveEsignSent(this.db, this.log) {
					CustomerID = nCustomerID,
					CashRequestID = cashRequestID,
					Directors = oAddressee.Where(x => x.PersonType == PersonType.Director).Select(x => x.ID).ToList(),
					ExperianDirectors = oAddressee
						.Where(x => x.PersonType == PersonType.ExperianDirector)
						.Select(x => x.ID).ToList(),
					DocumentKey = aryResult[0].documentKey,
					SentToCustomer = bSentToCustomer,
					TemplateID = nTemplateID,
				};

				sp.ExecuteNonQuery();
			} // if

			return EchoSignSendResultCode.Success;
		} // SendOne

		private void LoadConfiguration() {
			this.apiKey = CurrentValues.Instance.EchoSignApiKey;
			this.url = CurrentValues.Instance.EchoSignUrl;

			this.reminderFrequency = null;

			ReminderFrequency rf;
			if (Enum.TryParse(CurrentValues.Instance.EchoSignReminder, true, out rf))
				this.reminderFrequency = rf;

			try {
				this.deadline = CurrentValues.Instance.EchoSignDeadline;
			} catch (Exception) {
				this.deadline = -1;
			} // try

			if (this.deadline < 0)
				this.deadline = null;

			this.terminalStatuses = new HashSet<int>();

			this.db.ForEachRowSafe(
				(sr, bRowsetStart) => {
					if (!sr["IsTerminal"])
						return ActionResult.Continue;

					AgreementStatus nStatus;

					if (Enum.TryParse(sr["StatusName"], out nStatus))
						this.terminalStatuses.Add((int)nStatus);

					return ActionResult.Continue;
				},
				"LoadEsignAgreementStatuses",
				CommandSpecies.StoredProcedure
			);

			this.log.Debug("************************************************************************");
			this.log.Debug("*");
			this.log.Debug("* EchoSign façade configuration - begin:");
			this.log.Debug("*");
			this.log.Debug("************************************************************************");

			this.log.Debug("API key: {0}.", this.apiKey);
			this.log.Debug("URL: {0}.", this.url);
			this.log.Debug(
				"Reminder frequency: {0}.",
				this.reminderFrequency.HasValue ? this.reminderFrequency.Value.ToString() : "never"
			);
			this.log.Debug(
				"Signing deadline (days): {0}.",
				this.deadline.HasValue ? this.deadline.ToString() : "never"
			);
			this.log.Debug("Terminal statuses: {0}.", string.Join(", ", this.terminalStatuses));

			this.log.Debug("************************************************************************");
			this.log.Debug("*");
			this.log.Debug("* EchoSign façade configuration - end.");
			this.log.Debug("*");
			this.log.Debug("************************************************************************");
		} // LoadConfiguration

		private void CreateClient() {
			this.echoSign = new EchoSignClient(
				new BasicHttpsBinding {
					MaxBufferSize = 65536000,
					MaxReceivedMessageSize = 65536000,
					MaxBufferPoolSize = 524288,
				},
				new EndpointAddress(this.url)
			);

			this.log.Debug("EchoSign ping test...");

			Pong pong = this.echoSign.testPing(this.apiKey);

			if (pong.message == ExpectedPong)
				this.log.Debug("EchoSign ping succeeded.");
			else {
				this.log.Alert("EchoSign ping failed! Pong is: {0}", pong.message);
				this.isReady = false;
				return;
			} // if

			this.log.Debug("EchoSign echo test...");

			byte[] oTestData = Serialized.AsBase64(string.Format(
				"Current time is {0}.",
				DateTime.UtcNow.ToString("MMMM d yyyy HH:mm:ss", CultureInfo.InvariantCulture)
			));

			byte[] oTestResult = this.echoSign.testEchoFile(this.apiKey, oTestData);

			this.isReady = (oTestData.Length == oTestResult.Length) && oTestData.SequenceEqual(oTestResult);

			if (this.isReady)
				this.log.Debug("EchoSign echo succeeded.");
			else
				this.log.Alert("EchoSign echo failed!");
		} // CreateClient

		

		
	} // class EchoSignFacade
} // namespace
