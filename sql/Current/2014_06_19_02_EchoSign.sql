SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('EsignAgreementStatus') IS NULL
BEGIN
	CREATE TABLE EsignAgreementStatus (
		StatusID INT NOT NULL,
		StatusName NVARCHAR(64) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EsignAgreementStatus PRIMARY KEY (StatusID)
	)

	INSERT INTO EsignAgreementStatus(StatusName, StatusID) VALUES
		('OUT_FOR_SIGNATURE', 0),
		('WAITING_FOR_REVIEW', 1),
		('SIGNED', 2),
		('APPROVED', 3),
		('ABORTED', 4),
		('DOCUMENT_LIBRARY', 5),
		('WIDGET', 6),
		('EXPIRED', 7),
		('ARCHIVED', 8),
		('PREFILL', 9),
		('AUTHORING', 10),
		('WAITING_FOR_FAXIN', 11),
		('WAITING_FOR_VERIFICATION', 12),
		('WIDGET_WAITING_FOR_VERIFICATION', 13),
		('WAITING_FOR_PAYMENT', 14),
		('OUT_FOR_APPROVAL', 15),
		('OTHER', 16),
		('SIGNED_IN_ADOBE_ACROBAT', 17),
		('SIGNED_IN_ADOBE_READER', 18)
END
GO

IF OBJECT_ID('EsignUserAgreementStatus') IS NULL
BEGIN
	CREATE TABLE EsignUserAgreementStatus (
		StatusID INT NOT NULL,
		StatusName NVARCHAR(64) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EsignUserAgreementStatus PRIMARY KEY (StatusID)
	)

	INSERT INTO EsignUserAgreementStatus(StatusName, StatusID) VALUES
		('WAITING_FOR_MY_SIGNATURE', 0),
		('WAITING_FOR_MY_APPROVAL', 1),
		('OUT_FOR_SIGNATURE', 2),
		('SIGNED', 3),
		('APPROVED', 4),
		('RECALLED', 5),
		('HIDDEN', 6),
		('NOT_YET_VISIBLE', 7),
		('WAITING_FOR_FAXIN', 8),
		('ARCHIVED', 9),
		('UNKNOWN', 10),
		('PARTIAL', 11),
		('WAITING_FOR_AUTHORING', 13),
		('OUT_FOR_APPROVAL', 14),
		('WIDGET', 15),
		('EXPIRED', 16),
		('WAITING_FOR_MY_REVIEW', 17),
		('IN_REVIEW', 18),
		('OTHER', 19),
		('SIGNED_IN_ADOBE_ACROBAT', 20),
		('SIGNED_IN_ADOBE_READER', 21)
END
GO
