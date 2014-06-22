IF OBJECT_ID('SaveSignedDocument') IS NULL
	EXECUTE('CREATE PROCEDURE SaveSignedDocument AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SaveSignedDocument
@EsignatureID INT,
@DoSaveDoc BIT,
@StatusID INT,
@MimeType NVARCHAR(255),
@DocumentContent VARBINARY(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Esignatures SET
		StatusID = @StatusID,
		SignedDocumentMimeType = (CASE @DoSaveDoc WHEN 1 THEN @MimeType ELSE SignedDocumentMimeType END),
		SignedDocument = (CASE @DoSaveDoc WHEN 1 THEN @DocumentContent ELSE SignedDocument END)
	WHERE
		EsignatureID = @EsignatureID
END
GO
