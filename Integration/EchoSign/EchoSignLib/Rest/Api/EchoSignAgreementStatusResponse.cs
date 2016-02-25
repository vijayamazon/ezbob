namespace EchoSignLib.Rest.Api {
    using System;
    using EchoSignLib.Rest.Models;
    using EchoSignLib.Rest.Models.Enums;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    internal class EchoSignAgreementStatusResponse {
        /// <summary>
        /// //The message associated with the document that the sender has provided.
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// The current status of the document.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public EchoSignAgreementStatus status { get; set; }

        /// <summary>
        /// The name of the document, specified by the sender.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// A resource identifier that can be used to uniquely identify the agreement resource in other apis.
        /// </summary>
        public string agreementId { get; set; }

        /// <summary>
        /// Information about all the participant sets of this document.
        /// </summary>
        public EchoSignParticipantSetInfo[] participantSetInfos { get; set; }

        /// <summary>
        /// The date after which the document can no longer be signed, if an expiration date is configured. The value is nil if an expiration date is not set for the document.
        /// </summary>
        public DateTime? expiration { get; set; }

        /// <summary>
        /// An ordered list of the events in the audit trail of this document.
        /// </summary>
        public EchoSignDocumentHistoryEvent[] events { get; set; }
    }
}
