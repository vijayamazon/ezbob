using System;

namespace EchoSignLib.Rest.Api {
    internal class EchoSignSendAgreementResponse {
        public DateTime expiration { get; set; } //Expiration date for auto-login. This is based on the user setting, API_AUTO_LOGIN_LIFETIME
        public string agreementId { get; set; } // The unique identifier that can be used to query status and download signed documents
        public string embeddedCode { get; set; } //Javascript snippet suitable for an embedded page taking a user to a URL
        public string url { get; set; } //Standalone URL to direct end users to
    }
}
