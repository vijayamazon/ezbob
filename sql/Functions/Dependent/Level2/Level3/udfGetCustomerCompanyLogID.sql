IF OBJECT_ID('dbo.udfGetCustomerCompanyLogID') IS NOT NULL
	DROP FUNCTION dbo.udfGetCustomerCompanyLogID
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfGetCustomerCompanyLogID(@CustomerID INT)
RETURNS BIGINT
AS
BEGIN
	DECLARE @ServiceLogID BIGINT = (
		SELECT TOP 1
			ServiceLogID
		FROM
			dbo.udfGetCustomerHistoricalCompanyLogID(@CustomerID, NULL)
	)

	------------------------------------------------------------------------------

	RETURN @ServiceLogID
END
GO
