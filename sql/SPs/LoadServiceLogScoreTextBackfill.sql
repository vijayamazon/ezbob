IF OBJECT_ID('LoadServiceLogScoreTextBackfill') IS NULL
	EXECUTE('CREATE PROCEDURE LoadServiceLogScoreTextBackfill AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadServiceLogScoreTextBackfill
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Id,
		ResponseData
	FROM
		MP_ServiceLog
	WHERE
		ServiceType = 'E-SeriesLimitedData'
END
GO
