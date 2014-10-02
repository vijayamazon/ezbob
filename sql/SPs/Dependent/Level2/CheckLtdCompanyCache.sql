IF OBJECT_ID('CheckLtdCompanyCache') IS NULL
	EXECUTE('CREATE PROCEDURE CheckLtdCompanyCache AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE CheckLtdCompanyCache
@CompanyRefNum NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @ServiceLogID BIGINT
	DECLARE @InsertDate DATETIME = NULL
	
	------------------------------------------------------------------------------

	SELECT TOP 1
		@ServiceLogID = Id,
		@InsertDate = InsertDate
	FROM
		MP_ServiceLog l
		INNER JOIN ExperianLtd e ON l.Id = ServiceLogID
	WHERE
		l.CompanyRefNum = @CompanyRefNum
		AND
		l.ServiceType = 'E-SeriesLimitedData'
	ORDER BY
		l.InsertDate DESC,
		l.Id DESC
	
	------------------------------------------------------------------------------

	EXECUTE LoadFullExperianLtd @ServiceLogID, @InsertDate
END
GO
