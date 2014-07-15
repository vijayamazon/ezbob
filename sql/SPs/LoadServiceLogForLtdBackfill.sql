IF OBJECT_ID('LoadServiceLogForLtdBackfill') IS NULL
	EXECUTE('CREATE PROCEDURE LoadServiceLogForLtdBackfill AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadServiceLogForLtdBackfill
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Id
	FROM
		MP_ServiceLog
	WHERE
		ServiceType = 'E-SeriesLimitedData'
END
GO
