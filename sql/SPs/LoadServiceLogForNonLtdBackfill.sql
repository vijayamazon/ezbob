IF OBJECT_ID('LoadServiceLogForNonLtdBackfill') IS NULL
	EXECUTE('CREATE PROCEDURE LoadServiceLogForNonLtdBackfill AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadServiceLogForNonLtdBackfill
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Id,
		ResponseData
	FROM
		MP_ServiceLog
	WHERE
		ServiceType = 'E-SeriesNonLimitedData'
END
GO