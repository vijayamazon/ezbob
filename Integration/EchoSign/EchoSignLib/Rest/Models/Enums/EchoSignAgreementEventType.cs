namespace EchoSignLib.Rest.Models.Enums {
    /// <summary>
    /// Values are taken from SOAP api.
    /// New items: PRESIGNED, DIGSIGNED, AUTO_DELEGATED, AGREEMENT_MODIFIED, USER_ACK_AGREEMENT_MODIFIED
    /// </summary>
    public enum EchoSignAgreementEventType {
        SHARED = 14, //The document has been shared by a participant,
        DOCUMENTS_DELETED = 30, //Document retention applied - all documents deleted,
        SIGNER_SUGGESTED_CHANGES = 17, //Changes have been suggested by the signer on the document,
        EMAIL_BOUNCED = 24, //The Email sent to a signer bounced and was not delivered,
        SIGNED = 3, //The document has been signed,
        OTHER = 31, //In the future, statuses other than those above may be added to the Adobe Document Cloud application. For backward compatibility reasons, existing API clients receive status OTHER. You may need to update your client application to the latest version of the API to receive the new statuses in those cases,
        APPROVED = 5, //The document has been approved,
        EXPIRED_AUTOMATICALLY = 13, //The document automatically expired,
        VAULTED = 29, //Document was vaulted,
        APPROVAL_REQUESTED = 9, //The document has been sent out for approval,
        ESIGNED = 4, //The document has been eSigned,
        DELEGATED = 27, //The document has been delegated by the signer,
        AUTO_CANCELLED_CONVERSION_PROBLEM = 16, //The document has been cancelled because of problems with processing,
        FAXED_BY_SENDER = 2, //The document has been faxed in by the sender on behalf of the signer,
        PASSWORD_AUTHENTICATION_FAILED = 19, //Signer failed all password authentication attempts,
        KBA_AUTHENTICATED = 21, //Signer successfully verified identity using Knowledge Based Authentication,
        SIGNATURE_REQUESTED = 8, //The document has been sent out for signatures,
        EXPIRED = 12, //The document has expired,
        REJECTED = 11, //The document has been rejected by the signer,
        WEB_IDENTITY_AUTHENTICATED = 22, //Signer provided web identity before viewing the document,
        UPLOADED_BY_SENDER = 1, //The document has been uploaded by sender,
        WEB_IDENTITY_SPECIFIED = 23, //Signer provided web identity after viewing the document,
        WIDGET_DISABLED = 26, //The widget was disabled,
        CREATED = 0, //The document has been created,
        OFFLINE_SYNC = 6, //Offline events have been synchronized and recorded,
        REPLACED_SIGNER = 28, //Signer was replaced by the sender,
        WIDGET_ENABLED = 25, //The widget was enabled,
        EMAIL_VIEWED = 15, //The document has been viewed,
        RECALLED = 10, //The document has been cancelled by the sender,
        FAXIN_RECEIVED = 7, //The faxed-in signature has been received,
        SENDER_CREATED_NEW_REVISION = 18, //A new revision of the document has been created,
        KBA_AUTHENTICATION_FAILED = 20, //Signer failed all Knowledge Based Authentication authentication attempts

        USER_ACK_AGREEMENT_MODIFIED, //The agreement modification has been acknowledged,
        AGREEMENT_MODIFIED, //The agreement has been modified
        AUTO_DELEGATED, //The document has been automatically delegated by the signer,
        DIGSIGNED, //The document has been digitally Signed
        PRESIGNED, //The document was digitally Signed,
    }
}
