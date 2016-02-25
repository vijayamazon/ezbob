using System.Collections.Generic;
using System.Linq;

namespace EchoSignLib.Rest.Api {
    using System.Net.Http;
    using EchoSignLib.EchoSignService;
    using EchoSignLib.Rest.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Builds http content suitable for this request
    /// </summary>
    public class EchoSignSendAgreementRequest {
        private DocumentCreationInfo dci;
        private string transientDocumentId;
        /// <summary>
        /// Initializes a new instance of the <see cref="EchoSignSendAgreementRequest"/> class.
        /// </summary>
        /// <param name="dci">The dci.</param>
        public EchoSignSendAgreementRequest(DocumentCreationInfo dci) {
            this.dci = dci;
        }

        /// <summary>
        /// Sets the transient document identifier.
        /// </summary>
        /// <param name="transientDocId">The transient document identifier.</param>
        /// <returns></returns>
        public EchoSignSendAgreementRequest WithTransientDocId(string transientDocId) {
            this.transientDocumentId = transientDocId;
            return this;
        }

        /// <summary>
        /// Builds the content.
        /// </summary>
        /// <returns></returns>
        public HttpContent BuildContent() {
            string json = ConvertToJsonString(this.dci);
            return new JsonContent(json);
        }

        /// <summary>
        /// Converts to json string.
        /// </summary>
        /// <param name="dci">The dci.</param>
        /// <returns></returns>
        private string ConvertToJsonString(DocumentCreationInfo dci) {
            var createInfo = new EchoSignDocumentCreateInfo {
                name = dci.name,
                signatureType = "ESIGN",
                daysUntilSigningDeadline = dci.daysUntilSigningDeadline ?? 7,
                reminderFrequency = dci.reminderFrequency.HasValue ? dci.reminderFrequency.Value.ToString() : "DAILY_UNTIL_SIGNED",
                signatureFlow = dci.signatureFlow.HasValue ? dci.signatureFlow.Value.ToString() : "PARALLEL",
                recipientSetInfos = CreateRecipientSetInfo(dci.recipients)
                    .ToArray(),
                fileInfos = new[] {
                    CreateFileInfo()
                }
            };

            var agreementCreation = new EchoSignAgreementCreationInfo {
                documentCreationInfo = createInfo
            };

            return JsonConvert.SerializeObject(agreementCreation);
        }

        /// <summary>
        /// Creates the file information.
        /// </summary>
        /// <returns></returns>
        private EchoSignFileInfo CreateFileInfo() {
            return new EchoSignFileInfo {
                transientDocumentId = this.transientDocumentId
            };
        }

        /// <summary>
        /// Creates the recipient set information.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <returns></returns>
        private IEnumerable<EchoSignRecipientSetInfo> CreateRecipientSetInfo(RecipientInfo[] recipients) {
            yield return new EchoSignRecipientSetInfo {
                recipientSetMemberInfos = ConvertRecipients(recipients)
                    .ToArray(),
                recipientSetRole = "SIGNER"
            };
        }

        /// <summary>
        /// Converts the recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <returns></returns>
        private IEnumerable<EchoSignRecipientInfo> ConvertRecipients(RecipientInfo[] recipients) {
            foreach (var recipientInfo in recipients) {
                yield return ConvertRecipient(recipientInfo);
            }
        }

        /// <summary>
        /// Converts the recipient.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
        private EchoSignRecipientInfo ConvertRecipient(RecipientInfo recipient) {
            return new EchoSignRecipientInfo {
                email = recipient.email
            };
        }
    }
}
