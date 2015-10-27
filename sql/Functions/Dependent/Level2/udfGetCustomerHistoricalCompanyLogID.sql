IF OBJECT_ID('dbo.udfGetCustomerHistoricalCompanyLogID') IS NOT NULL
	DROP FUNCTION dbo.udfGetCustomerHistoricalCompanyLogID
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfGetCustomerHistoricalCompanyLogID(@CustomerID INT, @Now DATETIME)
RETURNS @output TABLE (
	CompanyRefNum NVARCHAR(50) NULL,
	ServiceLogID BIGINT NULL,
	TypeOfBusiness NVARCHAR(50) NULL
)
AS
BEGIN
	DECLARE @CompanyRefNum NVARCHAR(50)
	DECLARE @ServiceLogID BIGINT = NULL
	DECLARE @TypeOfBusiness NVARCHAR(50) = NULL

	------------------------------------------------------------------------------

	SELECT
		@CompanyRefNum = ISNULL(LTRIM(RTRIM(ISNULL(co.ExperianRefNum, ''))), ''),
		@TypeOfBusiness = co.TypeOfBusiness
	FROM
		Customer c
		LEFT JOIN Company co ON c.CompanyId = co.Id
	WHERE
		c.Id = @CustomerID

	------------------------------------------------------------------------------

	IF @CompanyRefNum IS NOT NULL AND @CompanyRefNum != ''
		SET @ServiceLogID = dbo.udfGetCompanyHistoricalLogID(@CompanyRefNum, @Now)

	------------------------------------------------------------------------------

	INSERT INTO @output (
		CompanyRefNum,
		ServiceLogID,
		TypeOfBusiness
	) VALUES (
		@CompanyRefNum,
		@ServiceLogID,
		@TypeOfBusiness
	)

	------------------------------------------------------------------------------

	RETURN
END
GO
