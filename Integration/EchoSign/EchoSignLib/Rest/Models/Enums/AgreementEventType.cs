using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoSignLib.Rest.Models.Enums
{
    enum AgreementEventType
    {
        CREATED = 0,
        UPLOADED_BY_SENDER = 1,
        FAXED_BY_SENDER = 2,
        SIGNED = 3,
        ESIGNED = 4,
        APPROVED = 5,
        OFFLINE_SYNC = 6,
        FAXIN_RECEIVED = 7,
        SIGNATURE_REQUESTED = 8,
        APPROVAL_REQUESTED = 9,
        RECALLED = 10,
        REJECTED = 11,
        EXPIRED = 12,
        EXPIRED_AUTOMATICALLY = 13,
        SHARED = 14,
        EMAIL_VIEWED = 15,
        AUTO_CANCELLED_CONVERSION_PROBLEM = 16,
        SIGNER_SUGGESTED_CHANGES = 17,
        SENDER_CREATED_NEW_REVISION = 18,
        PASSWORD_AUTHENTICATION_FAILED = 19,
        KBA_AUTHENTICATION_FAILED = 20,
        KBA_AUTHENTICATED = 21,
        WEB_IDENTITY_AUTHENTICATED = 22,
        WEB_IDENTITY_SPECIFIED = 23,
        EMAIL_BOUNCED = 24,
        WIDGET_ENABLED = 25,
        WIDGET_DISABLED = 26,
        DELEGATED = 27,
        REPLACED_SIGNER = 28,
        VAULTED = 29,
        DOCUMENTS_DELETED = 30,
        OTHER = 31,
    }

    /*
    SHARED (enum): The document has been shared by a participant,
DOCUMENTS_DELETED (enum): Document retention applied - all documents deleted,
SIGNER_SUGGESTED_CHANGES (enum): Changes have been suggested by the signer on the document,
EMAIL_BOUNCED (enum): The Email sent to a signer bounced and was not delivered,
SIGNED (enum): The document has been signed,
OTHER (enum): In the future, statuses other than those above may be added to the Adobe Document Cloud application. For backward compatibility reasons, existing API clients receive status OTHER. You may need to update your client application to the latest version of the API to receive the new statuses in those cases,
APPROVED (enum): The document has been approved,
EXPIRED_AUTOMATICALLY (enum): The document automatically expired,
VAULTED (enum): Document was vaulted,
PRESIGNED (enum): The document was digitally Signed,
APPROVAL_REQUESTED (enum): The document has been sent out for approval,
ESIGNED (enum): The document has been eSigned,
DELEGATED (enum): The document has been delegated by the signer,
AGREEMENT_MODIFIED (enum): The agreement has been modified,
AUTO_CANCELLED_CONVERSION_PROBLEM (enum): The document has been cancelled because of problems with processing,
FAXED_BY_SENDER (enum): The document has been faxed in by the sender on behalf of the signer,
PASSWORD_AUTHENTICATION_FAILED (enum): Signer failed all password authentication attempts,
DIGSIGNED (enum): The document has been digitally Signed,
KBA_AUTHENTICATED (enum): Signer successfully verified identity using Knowledge Based Authentication,
SIGNATURE_REQUESTED (enum): The document has been sent out for signatures,
EXPIRED (enum): The document has expired,
REJECTED (enum): The document has been rejected by the signer,
WEB_IDENTITY_AUTHENTICATED (enum): Signer provided web identity before viewing the document,
UPLOADED_BY_SENDER (enum): The document has been uploaded by sender,
WEB_IDENTITY_SPECIFIED (enum): Signer provided web identity after viewing the document,
WIDGET_DISABLED (enum): The widget was disabled,
CREATED (enum): The document has been created,
OFFLINE_SYNC (enum): Offline events have been synchronized and recorded,
REPLACED_SIGNER (enum): Signer was replaced by the sender,
AUTO_DELEGATED (enum): The document has been automatically delegated by the signer,
WIDGET_ENABLED (enum): The widget was enabled,
EMAIL_VIEWED (enum): The document has been viewed,
RECALLED (enum): The document has been cancelled by the sender,
FAXIN_RECEIVED (enum): The faxed-in signature has been received,
SENDER_CREATED_NEW_REVISION (enum): A new revision of the document has been created,
USER_ACK_AGREEMENT_MODIFIED (enum): The agreement modification has been acknowledged,
KBA_AUTHENTICATION_FAILED (enum): Signer failed all Knowledge Based Authentication authentication attempts
     */
}
