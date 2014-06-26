IF OBJECT_ID('LoadEsignatureFile') IS NULL
	EXECUTE('CREATE PROCEDURE LoadEsignatureFile AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadEsignatureFile
@EsignatureID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		tt.FileNameBase + '_' + CONVERT(NVARCHAR, s.EsignatureID),
		s.SignedDocumentMimeType AS MimeType,
		s.SignedDocument AS Contents
	FROM
		Esignatures s
		INNER JOIN EsignTemplates t ON s.EsignTemplateID = t.EsignTemplateID
		INNER JOIN EsignTemplateTypes tt ON t.EsignTemplateTypeID = tt.EsignTemplateTypeID
	WHERE
		s.EsignatureID = @EsignatureID
END
GO
