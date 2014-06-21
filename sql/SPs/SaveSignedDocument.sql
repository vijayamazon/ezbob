IF OBJECT_ID('SaveSignedDocument') IS NULL
	EXECUTE('CREATE PROCEDURE SaveSignedDocument AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SaveSignedDocument
@EsignatureID INT,
@MimeType NVARCHAR(255),
@DocumentContent VARBINARY(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Esignatures SET
		StatusID = 2,
		SignedDocumentMimeType = @MimeType,
		SignedDocument = @DocumentContent
	WHERE
		EsignatureID = @EsignatureID
END
GO
