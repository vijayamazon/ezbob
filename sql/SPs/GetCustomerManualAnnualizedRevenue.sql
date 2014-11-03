IF OBJECT_ID('GetCustomerManualAnnualizedRevenue') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerManualAnnualizedRevenue AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCustomerManualAnnualizedRevenue
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		CustomerID,
		EntryTime,
		AnnualizedRevenue AS Revenue,
		Comment
	FROM
		CustomerManualUwData
	WHERE
		CustomerID = @CustomerID
		AND
		IsActive = 1
END
GO
