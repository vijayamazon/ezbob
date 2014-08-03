IF OBJECT_ID('LoadCustomerLatestApprovedSum') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerLatestApprovedSum AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerLatestApprovedSum
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @ApprovedSum INT = 0
	
	------------------------------------------------------------------------------

	SELECT TOP 1
		cr.ManagerApprovedSum
	FROM
		CashRequests cr
	WHERE
		cr.IdCustomer = @CustomerID
		AND
		cr.ManagerApprovedSum IS NOT NULL
		AND
		cr.ManagerApprovedSum > 0
	ORDER BY
		cr.CreationDate DESC,
		cr.Id DESC
	
	------------------------------------------------------------------------------

	SELECT @ApprovedSum AS ApprovedSum
END
GO
