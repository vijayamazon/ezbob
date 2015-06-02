IF OBJECT_ID('BrokerLoadMarketingFiles') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadMarketingFiles AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadMarketingFiles
@OriginID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		f.BrokerMarketingFileID AS ID,
		f.FileID,
		f.FileName,
		f.DisplayName,
		f.MimeType,
		f.SortPosition
	FROM
		BrokerMarketingFile f
	WHERE
		f.IsActive = 1
		AND
		f.OriginID = @OriginID
	ORDER BY
		f.SortPosition
END
GO
