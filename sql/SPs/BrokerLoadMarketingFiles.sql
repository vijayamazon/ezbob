IF OBJECT_ID('BrokerLoadMarketingFiles') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadMarketingFiles AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadMarketingFiles
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		BrokerMarketingFileID AS ID,
		FileID,
		FileName,
		DisplayName,
		MimeType,
		SortPosition
	FROM
		BrokerMarketingFile
	WHERE
		IsActive = 1
	ORDER BY
		SortPosition
END
GO
