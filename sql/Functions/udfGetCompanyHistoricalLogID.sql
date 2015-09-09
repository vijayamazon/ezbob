SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfGetCompanyHistoricalLogID') IS NOT NULL
	DROP FUNCTION dbo.udfGetCompanyHistoricalLogID
GO

CREATE FUNCTION dbo.udfGetCompanyHistoricalLogID(@CompanyRefNum NVARCHAR(50), @Now DATETIME)
RETURNS BIGINT
AS
BEGIN
	DECLARE @ServiceLogID BIGINT = NULL

	SELECT TOP 1
		@ServiceLogID = l.Id
	FROM
		MP_ServiceLog l
	WHERE
		l.CompanyRefNum = @CompanyRefNum
		AND (
			@Now IS NULL OR l.InsertDate < @Now
		)
		AND
		l.ServiceType IN ('E-SeriesLimitedData', 'E-SeriesNonLimitedData')
	ORDER BY
		l.InsertDate DESC,
		l.Id DESC

	RETURN @ServiceLogID
END
GO
