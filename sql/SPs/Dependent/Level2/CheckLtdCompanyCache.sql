IF OBJECT_ID('CheckLtdCompanyCache') IS NULL
	EXECUTE('CREATE PROCEDURE CheckLtdCompanyCache AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE CheckLtdCompanyCache
@CompanyRefNum NVARCHAR(50),
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @UpdateCompanyDataPeriodDays INT
	DECLARE @ServiceLogID BIGINT
	
	------------------------------------------------------------------------------

	SELECT
		@UpdateCompanyDataPeriodDays = CONVERT(INT, Value)
	FROM
		ConfigurationVariables
	WHERE
		Name = 'UpdateCompanyDataPeriodDays'
	
	------------------------------------------------------------------------------

	SELECT TOP 1
		@ServiceLogID = Id
	FROM
		MP_ServiceLog l
	WHERE
		l.CompanyRefNum = @CompanyRefNum
		AND
		l.ServiceType = 'E-SeriesLimitedData'
		AND
		DATEDIFF(day, l.InsertDate, @Now) BETWEEN 0 AND @UpdateCompanyDataPeriodDays
	ORDER BY
		l.InsertDate DESC,
		l.Id DESC
	
	------------------------------------------------------------------------------

	EXECUTE LoadFullExperianLtd @ServiceLogID
END
GO
