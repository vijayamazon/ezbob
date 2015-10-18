SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfGetCustomerIncorporationDate') IS NOT NULL
	DROP FUNCTION dbo.udfGetCustomerIncorporationDate
GO

CREATE FUNCTION dbo.udfGetCustomerIncorporationDate(@CustomerID INT, @Now DATETIME)
RETURNS DATETIME
AS
BEGIN
	DECLARE @IncorporationDate DATETIME

	------------------------------------------------------------------------------

	DECLARE @CompanyRefNum NVARCHAR(50) = NULL
	DECLARE @ServiceLogID BIGINT = NULL
	DECLARE @TypeOfBusiness NVARCHAR(50) = NULL

	------------------------------------------------------------------------------

	SELECT
		@CompanyRefNum = CompanyRefNum,
		@ServiceLogID = ServiceLogID,
		@TypeOfBusiness = TypeOfBusiness
	FROM
		dbo.udfGetCustomerHistoricalCompanyLogID(@CustomerID, @Now)

	------------------------------------------------------------------------------

	IF @TypeOfBusiness IN ('Limited', 'LLP')
	BEGIN
		SELECT
			@IncorporationDate = IncorporationDate
		FROM
			ExperianLtd
		WHERE
			ServiceLogID = @ServiceLogID
	END
	ELSE BEGIN
		SELECT
			@IncorporationDate = IncorporationDate
		FROM
			ExperianNonLimitedResults
		WHERE
			ServiceLogID = @ServiceLogID
	END

	------------------------------------------------------------------------------

	RETURN @IncorporationDate
END
GO
