SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfGetCustomerCompanyID') IS NOT NULL
	DROP FUNCTION dbo.udfGetCustomerCompanyID
GO

CREATE FUNCTION dbo.udfGetCustomerCompanyID(@CustomerID INT, @Now DATETIME)
RETURNS INT
AS
BEGIN
	DECLARE @CompanyID INT = NULL
	
	IF @Now IS NOT NULL
	BEGIN
		SELECT TOP 1
			@CompanyID = CompanyId
		FROM 
			CustomerCompanyHistory
		WHERE
			CustomerId = @CustomerID
			AND
			InsertDate < @Now
		ORDER BY
			InsertDate DESC,
			Id DESC
	END

	IF @CompanyID IS NULL
		SELECT @CompanyID = CompanyID FROM Customer WHERE Id = @CustomerID

	RETURN @CompanyID
END
GO
