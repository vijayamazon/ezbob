IF OBJECT_ID('ClearCustomerAnalyticsCompany') IS NULL
	EXECUTE('CREATE PROCEDURE ClearCustomerAnalyticsCompany AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE ClearCustomerAnalyticsCompany
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE CustomerAnalyticsCompany SET
		IsActive = 0
	WHERE
		CustomerID = @CustomerID
		AND
		ISActive = 1
END
GO
