SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveSignedDocument') IS NOT NULL
	DROP PROCEDURE SaveSignedDocument
GO

IF TYPE_ID('EsignHistoryEvent') IS NOT NULL
	DROP TYPE EsignHistoryEvent
GO

CREATE TYPE EsignHistoryEvent AS TABLE (
	 EventTime            DATETIME NOT NULL,
	 Description          NTEXT NULL,
	 VersionKey           NVARCHAR(255) NULL,
	 EventTypeID          INT NOT NULL,
	 ActingUserEmail      NVARCHAR(255) NULL,
	 ActingUserIp         NVARCHAR(255) NULL,
	 ParticipantEmail     NVARCHAR(255) NULL,
	 Comment              NTEXT NULL,
	 Latitude             REAL NULL,
	 Longitude            REAL NULL,
	 SynchronizationKey   NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveSignedDocument
@EsignatureID INT,
@DoSaveDoc BIT,
@StatusID INT,
@MimeType NVARCHAR(255),
@DocumentContent VARBINARY(MAX),
@SignerStatuses EsignerStatus READONLY,
@HistoryEvents EsignHistoryEvent READONLY
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION

	UPDATE Esignatures SET
		StatusID = @StatusID,
		SignedDocumentMimeType = (CASE @DoSaveDoc WHEN 1 THEN @MimeType ELSE SignedDocumentMimeType END),
		SignedDocument = (CASE @DoSaveDoc WHEN 1 THEN @DocumentContent ELSE SignedDocument END)
	WHERE
		EsignatureID = @EsignatureID

	UPDATE Esigners SET
		StatusID = n.StatusID,
		SignDate = n.SignatureTime
	FROM
		Esigners s
		INNER JOIN @SignerStatuses n ON s.EsignerID = n.EsignerID

	DELETE FROM EsignatureHistory WHERE EsignatureID = @EsignatureID

	INSERT INTO EsignatureHistory (
		EsignatureID,
		EventTime,
		Description,
		VersionKey,
		EventTypeID,
		ActingUserEmail,
		ActingUserIp,
		ParticipantEmail,
		Comment,
		Latitude,
		Longitude,
		SynchronizationKey
	) SELECT
		@EsignatureID,
		EventTime,
		Description,
		VersionKey,
		EventTypeID,
		ActingUserEmail,
		ActingUserIp,
		ParticipantEmail,
		Comment,
		Latitude,
		Longitude,
		SynchronizationKey
	FROM
		@HistoryEvents

	COMMIT TRANSACTION
END
GO
