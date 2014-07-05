SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('EsignAgreementEventType') IS NULL
BEGIN
	CREATE TABLE EsignAgreementEventType (
		EventTypeID INT NOT NULL,
		EventType NVARCHAR(64) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EsignAgreementEventType PRIMARY KEY (EventTypeID)
	)

	INSERT INTO EsignAgreementEventType(EventType, EventTypeID) VALUES
        ('CREATED', 0),
        ('UPLOADED_BY_SENDER', 1),
        ('FAXED_BY_SENDER', 2),
        ('SIGNED', 3),
        ('ESIGNED', 4),
        ('APPROVED', 5),
        ('OFFLINE_SYNC', 6),
        ('FAXIN_RECEIVED', 7),
        ('SIGNATURE_REQUESTED', 8),
        ('APPROVAL_REQUESTED', 9),
        ('RECALLED', 10),
        ('REJECTED', 11),
        ('EXPIRED', 12),
        ('EXPIRED_AUTOMATICALLY', 13),
        ('SHARED', 14),
        ('EMAIL_VIEWED', 15),
        ('AUTO_CANCELLED_CONVERSION_PROBLEM', 16),
        ('SIGNER_SUGGESTED_CHANGES', 17),
        ('SENDER_CREATED_NEW_REVISION', 18),
        ('PASSWORD_AUTHENTICATION_FAILED', 19),
        ('KBA_AUTHENTICATION_FAILED', 20),
        ('KBA_AUTHENTICATED', 21),
        ('WEB_IDENTITY_AUTHENTICATED', 22),
        ('WEB_IDENTITY_SPECIFIED', 23),
        ('EMAIL_BOUNCED', 24),
        ('WIDGET_ENABLED', 25),
        ('WIDGET_DISABLED', 26),
        ('DELEGATED', 27),
        ('REPLACED_SIGNER', 28),
        ('VAULTED', 29),
        ('DOCUMENTS_DELETED', 30),
        ('OTHER', 31)
END
GO
