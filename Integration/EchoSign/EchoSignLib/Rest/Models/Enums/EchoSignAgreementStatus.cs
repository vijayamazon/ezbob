namespace EchoSignLib.Rest.Models.Enums {

    /// <summary>
    /// The values are taken from SOAP api.
    ///  SIGNED_IN_ADOBE_ACROBAT=17, SIGNED_IN_ADOBE_READER=18 - not exist in REST api. 
    /// </summary>
    internal enum EchoSignAgreementStatus {
        ABORTED = 4, //The signature workflow has been cancelled by either the sender or the recipient. This is a terminal state,
        EXPIRED = 7, //The agreement has passed the expiration date and can no longer be signed. This is a terminal state,
        SIGNED = 2, //The agreement has been signed by all the requested parties. This is a terminal state,
        DOCUMENT_LIBRARY = 5, //The status for agreements that are in the user's template library. This is a terminal state,
        OTHER = 16, //In the future, statuses other than those above may be added to the Adobe Document Cloud application. For backward compatibility reasons, existing API clients will receive status OTHER. You may need to update your client application to the latest version of the API to receive the new statuses in those cases,
        ARCHIVED = 8, //The agreement uploaded by the user into their document archive. This is a terminal state,
        OUT_FOR_SIGNATURE = 0, //The agreement is out for signature,
        WIDGET_WAITING_FOR_VERIFICATION = 13, //The widget is currently waiting to be verified,
        APPROVED = 3, //The agreement has been approved by all requested parties. If agreement has both signers and approvers, terminal status will be signed,
        WIDGET = 6, //The status for the user's widgets. This is a terminal state,
        AUTHORING = 10, //The agreement is waiting for the sender to position fields before it can be sent for signature,
        PREFILL = 9, //The agreement is waiting for the sender to fill out fields before it can be sent for signature,
        OUT_FOR_APPROVAL = 15, //The agreement out for approval,
        WAITING_FOR_REVIEW = 1, //The agreement is currently waiting to be reviewed,
        WAITING_FOR_PAYMENT = 14, //The agreement is waiting for payment in order to proceed,
        WAITING_FOR_VERIFICATION = 12, //The agreement is currently waiting to be verified,
        WAITING_FOR_FAXIN = 11 //The agreement is waiting for the sender to fax in the document contents before it can be sent for signature
    }
}
