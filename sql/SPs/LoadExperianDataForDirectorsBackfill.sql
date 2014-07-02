IF OBJECT_ID('LoadExperianDataForDirectorsBackfill') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianDataForDirectorsBackfill AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianDataForDirectorsBackfill
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		l.Id,
		l.CustomerID,
		l.ResponseData,
		l.ServiceType
	FROM
		MP_ServiceLog l
	WHERE
		l.CustomerId IS NOT NULL
		AND
		l.ServiceType IN ('E-SeriesLimitedData', 'E-SeriesNonLimitedData')
		AND
		(@CustomerID IS NULL OR l.CustomerId = @CustomerID)
	ORDER BY
		l.InsertDate
END
GO
