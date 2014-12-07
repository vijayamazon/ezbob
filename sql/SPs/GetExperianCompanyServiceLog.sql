SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetExperianCompanyServiceLog') IS NULL
	EXECUTE('CREATE PROCEDURE GetExperianCompanyServiceLog AS SELECT 1')
GO

ALTER PROCEDURE GetExperianCompanyServiceLog
@CustomerId INT,
@ServiceLogId BIGINT OUTPUT,
@Now DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE
		@CompanyID INT,
		@RefNum NVARCHAR(50)

	SELECT
		@CompanyID = co.Id,
		@RefNum = co.ExperianRefNum
	FROM
		Company co
		INNER JOIN Customer c ON co.Id = c.CompanyId

	SELECT TOP 1
		@ServiceLogID = Id
	FROM
		MP_ServiceLog l
	WHERE (
			l.CompanyRefNum = @RefNum
			OR
			l.CompanyID = @CompanyID
		)
		AND
		l.ServiceType LIKE 'E-%' -- only company data corresponds to this (E-SeriesLimitedData, E-SeriesNonLimitedData), targeting is ESeriesTargeting
		AND
		(@Now IS NULL OR l.InsertDate < @Now)
	ORDER BY
		l.InsertDate DESC,
		l.Id DESC
END
GO
