IF OBJECT_ID('dbo.udfGetCustomerCompanyLogID') IS NOT NULL
	DROP FUNCTION dbo.udfGetCustomerCompanyLogID
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfGetCustomerCompanyLogID(@CustomerID INT)
RETURNS BIGINT
AS
BEGIN
	DECLARE @CompanyRefNum NVARCHAR(50)
	DECLARE @ServiceLogID BIGINT = NULL

	------------------------------------------------------------------------------

	SELECT
		@CompanyRefNum = ISNULL(LTRIM(RTRIM(ISNULL(co.ExperianRefNum, ''))), '')
	FROM
		Customer c
		INNER JOIN Company co ON c.CompanyId = co.Id
	WHERE
		c.Id = @CustomerID

	------------------------------------------------------------------------------

	IF @CompanyRefNum IS NOT NULL AND @CompanyRefNum != ''
	BEGIN
		SELECT TOP 1
			@ServiceLogID = Id
		FROM
			MP_ServiceLog
		WHERE
			CompanyRefNum = @CompanyRefNum
		ORDER BY
			InsertDate DESC,
			Id DESC
	END
	
	------------------------------------------------------------------------------

	RETURN @ServiceLogID
END
GO
