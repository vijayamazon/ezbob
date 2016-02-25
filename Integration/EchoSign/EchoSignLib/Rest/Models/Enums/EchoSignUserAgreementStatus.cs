namespace EchoSignLib.Rest.Models.Enums {
    /// <summary>
    /// Values are taken from SOAP API
    /// </summary>
    internal enum EchoSignUserAgreementStatus {
        EXPIRED = 16, //The agreement has expired,
        SIGNED = 3, //The agreement has been signed and filed,
        WAITING_FOR_MY_APPROVAL = 1, //The agreement is waiting for the current participant to approve,
        OTHER = 19, //In the future, statuses other than those above may be added to the Adobe Document Cloud application. For backward compatibility reasons, existing API clients will receive status OTHER. You may need to update your client application to the latest version of the API to receive the new statuses in those cases,
        UNKNOWN = 10, //The current status of the agreement is unknown,
        ARCHIVED = 9, //The agreement has been archived,
        OUT_FOR_SIGNATURE = 2, //The agreement is out for signature,
        APPROVED = 4, //The agreement has been approved,
        HIDDEN = 6, //The agreement is currently hidden,
        WAITING_FOR_MY_DELEGATION, //The agreement is waiting for the current participant to delegate,
        WAITING_FOR_MY_SIGNATURE = 0, //The agreement is waiting for the current participant to sign,
        WAITING_FOR_AUTHORING = 13, //The agreement is waiting to be authored,
        WIDGET = 15, //The agreement is a widget,
        OUT_FOR_APPROVAL = 14, //The agreement out for approval,
        RECALLED = 5, //The agreement has been recalled by the sender,
        FORM = 12, //The agreement is a form,
        NOT_YET_VISIBLE = 7, //The agreement is not yet visible to the current participant,
        IN_REVIEW = 18, //The agreement is in the review process,
        WAITING_FOR_MY_REVIEW = 17, //The agreement is waiting for the current participant to review,
        PARTIAL = 11, //The agreement is incomplete,
        WAITING_FOR_FAXIN = 8 //The agreement is waiting for the signature to be faxed-in
    }
}
