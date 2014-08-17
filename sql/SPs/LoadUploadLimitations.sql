IF OBJECT_ID('LoadUploadLimitations') IS NULL
	EXECUTE('CREATE PROCEDURE LoadUploadLimitations AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadUploadLimitations
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		UploadLimitationID,
		ControllerName,
		ActionName,
		FileSizeLimit,
		AcceptedFiles
	FROM
		UploadLimitations
	ORDER BY
		ControllerName,
		ActionName
END
GO
