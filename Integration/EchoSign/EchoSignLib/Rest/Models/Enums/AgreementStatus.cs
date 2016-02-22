namespace EchoSignLib.Rest.Models.Enums {

    /// <summary>
    /// The values of enums are taken from SOAP api enum.
    /// </summary>
    internal enum AgreementStatus
    {
        OUT_FOR_SIGNATURE = 0,
        WAITING_FOR_REVIEW = 1,
        SIGNED = 2,
        APPROVED = 3,
        ABORTED = 4,
        DOCUMENT_LIBRARY = 5,
        WIDGET = 6,
        EXPIRED = 7,
        ARCHIVED = 8,
        PREFILL = 9,
        AUTHORING = 10,
        WAITING_FOR_FAXIN = 11,
        WAITING_FOR_VERIFICATION = 12,
        WIDGET_WAITING_FOR_VERIFICATION = 13,
        WAITING_FOR_PAYMENT = 14,
        OUT_FOR_APPROVAL = 15,
        OTHER = 16
    }
}
