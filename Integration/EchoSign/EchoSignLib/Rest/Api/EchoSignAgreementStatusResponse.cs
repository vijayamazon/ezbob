namespace EchoSignLib.Rest.Api {
    using EchoSignLib.Rest.Models;
    using EchoSignLib.Rest.Models.Enums;

    internal class EchoSignAgreementStatusResponse {
        public string message { get; set; } //The message associated with the document that the sender has provided
        public AgreementStatus status { get; set; } //The current status of the document
        //public EchoSignService.AgreementStatus
        public string name { get; set; } //The name of the document, specified by the sender
        public string agreementId { get; set; } //A resource identifier that can be used to uniquely identify the agreement resource in other apis
        public EchoSignRecipientInfo[] participantSetInfos; //we use mail so reusing the same model 'EchoSignRecipientInfo' for participants
    }
}
