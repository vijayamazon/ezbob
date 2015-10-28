IF OBJECT_ID('GetIsCompanyDataUpdated') IS NULL
	EXECUTE('CREATE PROCEDURE GetIsCompanyDataUpdated AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetIsCompanyDataUpdated
@CustomerId INT,
@Today DATE
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @RefNum NVARCHAR(50) = (
		SELECT
			co.ExperianRefNum
		FROM
			Customer c
			INNER JOIN Company co ON c.CompanyId = co.Id
		WHERE
			c.Id = @CustomerId
	)

	DECLARE @LastUpdateTime DATETIME = (
		SELECT MAX(InsertDate)
		FROM MP_ServiceLog
		WHERE CompanyRefNum = @RefNum
		AND ServiceType IN ('E-SeriesLimitedData', 'E-SeriesNonLimitedData')
	)

	IF @LastUpdateTime IS NULL
	BEGIN
		SELECT CAST (0 AS BIT) AS IsUpdated
		RETURN
	END

	IF @Today <= @LastUpdateTime
		SELECT CAST (1 AS BIT) AS IsUpdated
	ELSE
		SELECT CAST (0 AS BIT) AS IsUpdated
END
GO
